using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CMC.Web.Services;

namespace CMC.Web.Util;

public sealed class EditContextAdapter
{
	public EFEditRequest Request { get; }
	public object Model { get; }
	private readonly Dictionary<string, object?> _overrides;

	public EditContextAdapter(EFEditRequest request, object model)
	{
		Request = request ?? throw new ArgumentNullException(nameof(request));
		Model = model ?? throw new ArgumentNullException(nameof(model));
		_overrides = new(StringComparer.OrdinalIgnoreCase);
	}

	public EditContextAdapter(EFEditRequest request, object model, IDictionary<string, object?>? overrides)
	{
		Request = request ?? throw new ArgumentNullException(nameof(request));
		Model = model ?? throw new ArgumentNullException(nameof(model));
		_overrides = overrides as Dictionary<string, object?>
			?? (overrides != null
				? new Dictionary<string, object?>(overrides, StringComparer.OrdinalIgnoreCase)
				: new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));
	}

	/// <summary>Optional: einzelne Feldwerte setzen/überschreiben (falls dein FormRenderer das nutzt).</summary>
	public void Set(string name, object? value)
	{
		if (string.IsNullOrWhiteSpace(name)) return;
		_overrides[name] = value;
	}

	/// <summary>
	/// Baut per Konvention den Request-Typ:
	/// Kandidaten: {action}{ModelName}Request, {action}{ModelName}, {ModelName}{action}Request, {ModelName}{action}
	/// Beispiel: Model = UserDto, action="Register" -> RegisterUserRequest
	/// </summary>
	public object Build(string action)
	{
		if (Request.ContractsAssembly == null)
			throw new InvalidOperationException("ContractsAssembly ist nicht gesetzt.");

		var modelName = StripSuffix(Model.GetType().Name, "Dto");
		var candidates = new[]
		{
			$"{action}{modelName}Request",
			$"{action}{modelName}",
			$"{modelName}{action}Request",
			$"{modelName}{action}"
		};

		var targetType = Request.ContractsAssembly
			.GetTypes()
			.FirstOrDefault(t => candidates.Any(c => string.Equals(t.Name, c, StringComparison.OrdinalIgnoreCase)));

		if (targetType == null)
			throw new InvalidOperationException($"Kein Request-Typ für Aktion '{action}' und Model '{modelName}' gefunden (gesucht: {string.Join(", ", candidates)}).");

		// ---------------- Wert-Vorrat zusammenstellen (Model -> ExtraFields -> Overrides) ----------------
		var bag = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

		foreach (var sp in Model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!sp.CanRead) continue;
			bag[sp.Name] = sp.GetValue(Model);
		}

		if (Request.ExtraFields is { Count: > 0 })
		{
			foreach (var ef in Request.ExtraFields)
				bag[ef.Name] = ef.Value;
		}

		if (_overrides.Count > 0)
		{
			foreach (var kv in _overrides)
				bag[kv.Key] = kv.Value;
		}

		// ---------------- Instanz anlegen ----------------
		object? instance = TryCreateWithParameterlessCtor(targetType)
						   ?? TryCreateWithBestMatchingCtor(targetType, bag)
						   ?? CreateUninitializedInstance(targetType); // modernized approach

		// ---------------- Properties nachziehen (nur schreibbar & nicht init-only) ----------------
		MapByName(bag, instance);

		return instance!;
	}

	// ========================= Erzeugungs-Strategien =========================

	private static object? TryCreateWithParameterlessCtor(Type targetType)
	{
		var hasDefault = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
								   .Any(c => c.GetParameters().Length == 0);
		return hasDefault ? Activator.CreateInstance(targetType) : null;
	}

	private object? TryCreateWithBestMatchingCtor(Type targetType, IDictionary<string, object?> bag)
	{
		var ctors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		if (ctors.Length == 0) return null;

		// Score: wie viele Parameter können wir befüllen (oder haben Default)?
		var best = ctors
			.Select(c =>
			{
				var ps = c.GetParameters();
				int matched = 0;
				bool allBindable = true;

				foreach (var p in ps)
				{
					if (bag.ContainsKey(p.Name!)) { matched++; continue; }
					if (p.HasDefaultValue) continue;

					// Werttypen ohne Default sind nicht bindbar
					if (Nullable.GetUnderlyingType(p.ParameterType) is null && p.ParameterType.IsValueType)
					{
						allBindable = false;
						break;
					}
				}

				return new { Ctor = c, Params = ps, Matched = matched, AllBindable = allBindable };
			})
			// nur die, die wir vollständig befüllen können (oder Defaults/Nulle zulassen)
			.Where(x => x.AllBindable)
			.OrderByDescending(x => x.Matched)
			.ThenBy(x => x.Params.Length)
			.FirstOrDefault();

		if (best is null) return null;

		var args = new object?[best.Params.Length];
		for (int i = 0; i < best.Params.Length; i++)
		{
			var p = best.Params[i];
			if (bag.TryGetValue(p.Name!, out var raw))
			{
				args[i] = ConvertToType(raw, p.ParameterType);
			}
			else if (p.HasDefaultValue)
			{
				args[i] = p.DefaultValue;
			}
			else
			{
				// null oder default(T) für nicht-bindbare optionale
				args[i] = GetNullDefault(p.ParameterType);
			}
		}

		return best.Ctor.Invoke(args);
	}

	/// <summary>
	/// Creates an uninitialized instance using modern approach instead of obsolete FormatterServices.
	/// </summary>
	private static object CreateUninitializedInstance(Type targetType)
	{
		try
		{
			// 1) Public oder private parameterloser Ctor
			return Activator.CreateInstance(targetType, nonPublic: true)!;
		}
		catch
		{
			try
			{
				// 2) Modern: kein FormatterServices mehr -> keine SYSLIB0050
				return RuntimeHelpers.GetUninitializedObject(targetType);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					$"Cannot create instance of type '{targetType.FullName}'. " +
					"The type must have a parameterless constructor or support uninitialized creation.",
					ex);
			}
		}
	}

	// Alternative modern approach using RuntimeHelpers (available in .NET 5+)
	// Uncomment this method and use it in CreateUninitializedInstance if you're on .NET 5+
	/*
	private static object CreateUninitializedInstanceModern(Type targetType)
	{
		try
		{
			// First try: Activator.CreateInstance with nonPublic flag
			return Activator.CreateInstance(targetType, nonPublic: true)!;
		}
		catch
		{
			try
			{
				// Modern approach: RuntimeHelpers.GetUninitializedObject (.NET 5+)
				return System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(targetType);
			}
			catch
			{
				throw new InvalidOperationException(
					$"Cannot create instance of type '{targetType.FullName}'. " +
					"The type must support object creation through Activator or RuntimeHelpers.");
			}
		}
	}
	*/

	// ========================= Mapping Helpers =========================

	/// <summary>
	/// Werte aus Bag per Name auf das Zielobjekt mappen (nur schreibbare & nicht init-only Properties).
	/// </summary>
	private static bool IsNavigationProperty(PropertyInfo prop)
	{
		// Collections (außer string) sind Navigation Properties
		if (prop.PropertyType != typeof(string) &&
			typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
			return true;

		// Komplexe Typen (nicht primitive, Guid, DateTime, etc.) sind Navigation Properties
		if (!prop.PropertyType.IsPrimitive &&
			prop.PropertyType != typeof(string) &&
			prop.PropertyType != typeof(Guid) &&
			prop.PropertyType != typeof(Guid?) &&
			prop.PropertyType != typeof(DateTime) &&
			prop.PropertyType != typeof(DateTime?) &&
			prop.PropertyType != typeof(DateTimeOffset) &&
			prop.PropertyType != typeof(DateTimeOffset?) &&
			prop.PropertyType != typeof(decimal) &&
			prop.PropertyType != typeof(decimal?) &&
			!prop.PropertyType.IsEnum &&
			!Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)
			return true;

		return false;
	}

	private static bool TrySet(object target, string propName, object? value)
	{
		var p = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (p == null || !p.CanWrite || IsInitOnly(p)) return false;

		var converted = ConvertToType(value, p.PropertyType);
		p.SetValue(target, converted);
		return true;
	}

	private static bool IsInitOnly(PropertyInfo p)
	{
		var set = p.SetMethod;
		if (set is null) return true;
		// init-only: hat den Modifizierer IsExternalInit
		var mods = set.ReturnParameter?.GetRequiredCustomModifiers() ?? Type.EmptyTypes;
		return mods.Contains(typeof(IsExternalInit));
	}

	private static object? ConvertToType(object? value, Type targetType)
	{
		if (value == null) return GetNullDefault(targetType);

		var nonNullTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

		// Bereits passend?
		if (nonNullTarget.IsInstanceOfType(value)) return value;

		// String-Quelle?
		if (value is string s)
		{
			if (nonNullTarget == typeof(Guid))
				return Guid.TryParse(s, out var g) ? g : GetNullDefault(targetType);

			if (nonNullTarget == typeof(DateTime))
				return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : GetNullDefault(targetType);

			if (nonNullTarget.IsEnum)
				return Enum.TryParse(nonNullTarget, s, true, out var ev) ? ev : GetNullDefault(targetType);

			// Zahlen / bool
			try
			{
				return System.Convert.ChangeType(s, nonNullTarget, CultureInfo.InvariantCulture);
			}
			catch { /* fallthrough */ }
		}

		// IEnumerable<string> -> List<Guid> / List<int> / ...
		if (IsGenericList(nonNullTarget, out var itemType))
		{
			if (value is IEnumerable enumerable)
			{
				var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
				foreach (var obj in enumerable)
				{
					var elem = obj;
					if (obj is string es)
					{
						elem = ConvertStringTo(es, itemType);
					}
					else if (obj != null && !itemType.IsInstanceOfType(obj))
					{
						elem = System.Convert.ChangeType(obj, itemType, CultureInfo.InvariantCulture);
					}
					list.Add(elem!);
				}
				return list;
			}
		}

		// Alles andere: ChangeType versuchen
		try
		{
			return System.Convert.ChangeType(value, nonNullTarget, CultureInfo.InvariantCulture);
		}
		catch
		{
			return GetNullDefault(targetType);
		}
	}

	private static object? ConvertStringTo(string s, Type itemType)
	{
		var nn = Nullable.GetUnderlyingType(itemType) ?? itemType;

		if (nn == typeof(Guid))
			return Guid.TryParse(s, out var g) ? g : GetNullDefault(itemType);

		if (nn == typeof(DateTime))
			return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : GetNullDefault(itemType);

		if (nn.IsEnum)
			return Enum.TryParse(nn, s, true, out var ev) ? ev : GetNullDefault(itemType);

		try
		{
			return System.Convert.ChangeType(s, nn, CultureInfo.InvariantCulture);
		}
		catch
		{
			return GetNullDefault(itemType);
		}
	}

	private static bool IsGenericList(Type t, out Type itemType)
	{
		itemType = typeof(object);
		if (!t.IsGenericType) return false;
		if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;
		itemType = t.GetGenericArguments()[0];
		return true;
	}

	private static object? GetNullDefault(Type t)
	{
		var nn = Nullable.GetUnderlyingType(t) ?? t;
		if (nn.IsValueType && Nullable.GetUnderlyingType(t) == null)
		{
			// non-nullable value type -> default(T)
			return Activator.CreateInstance(nn);
		}
		return null;
	}

	private static string StripSuffix(string name, string suffix)
	{
		return name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
			? name[..^suffix.Length]
			: name;
	}
}
