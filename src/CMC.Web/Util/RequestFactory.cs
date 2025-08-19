using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CMC.Web.Util;

public static class RequestFactory
{
	public static object MapByProvider(object model, IValueProvider provider, string action, Assembly contractsAssembly)
	{
		Console.WriteLine($"=== RequestFactory.MapByProvider ===");
		Console.WriteLine($"Action: {action}");
		Console.WriteLine($"Model: {model.GetType().Name}");

		var requestType = RequestConventions.FindRequestType(model, action, contractsAssembly)
			?? throw new InvalidOperationException($"Request-Typ für '{action}' nicht gefunden.");

		Console.WriteLine($"Found request type: {requestType.Name}");

		return MapToRequest(model, provider, requestType);
	}

	private static object MapToRequest(object model, IValueProvider provider, Type requestType)
	{
		Console.WriteLine($"=== MapToRequest for {requestType.Name} ===");

		var ctors = requestType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
			.OrderByDescending(c => c.GetParameters().Length)
			.ToList();

		Console.WriteLine($"Found {ctors.Count} constructors");

		// Versuche erst Konstruktoren mit Parametern
		foreach (var ctor in ctors.Where(c => c.GetParameters().Length > 0))
		{
			var ps = ctor.GetParameters();
			var args = new object?[ps.Length];
			var ok = true;

			Console.WriteLine($"Trying constructor with {ps.Length} parameters:");

			for (int i = 0; i < ps.Length; i++)
			{
				var p = ps[i];
				Console.WriteLine($"  Parameter {i}: {p.Name} ({p.ParameterType.Name})");

				if (string.IsNullOrEmpty(p.Name) || !TryGet(provider, model, p.Name, out var val))
				{
					if (p.HasDefaultValue)
					{
						args[i] = p.DefaultValue;
						Console.WriteLine($"    -> Using default value: {p.DefaultValue}");
						continue;
					}
					if (IsNullable(p.ParameterType))
					{
						args[i] = null;
						Console.WriteLine($"    -> Using null (nullable)");
						continue;
					}
					Console.WriteLine($"    -> FAILED: No value found and not nullable/no default");
					ok = false;
					break;
				}

				Console.WriteLine($"    -> Found value: '{val}' (type: {val?.GetType().Name ?? "null"})");

				if (!TryConvert(val, p.ParameterType, out var conv))
				{
					Console.WriteLine($"    -> FAILED: Could not convert to {p.ParameterType.Name}");
					ok = false;
					break;
				}

				Console.WriteLine($"    -> Converted to: '{conv}' (type: {conv?.GetType().Name ?? "null"})");
				args[i] = conv;
			}

			if (ok)
			{
				Console.WriteLine($"Constructor with parameters succeeded! Creating instance...");
				var instance = ctor.Invoke(args);
				Console.WriteLine($"Created: {instance}");
				return instance;
			}
			else
			{
				Console.WriteLine($"Constructor failed, trying next...");
			}
		}

		Console.WriteLine("All parameterized constructors failed, using parameterless + property setting...");

		// Fallback: parameterlos + setzbare Properties
		var inst = Activator.CreateInstance(requestType) ?? throw new InvalidOperationException($"Cannot create {requestType.Name}");
		var props = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

		Console.WriteLine($"Found {props.Count()} writable properties");

		foreach (var rp in props)
		{
			Console.WriteLine($"Setting property: {rp.Name}");
			if (string.IsNullOrEmpty(rp.Name) || !TryGet(provider, model, rp.Name, out var val))
			{
				Console.WriteLine($"  -> No value found");
				continue;
			}
			Console.WriteLine($"  -> Found value: '{val}'");

			if (!TryConvert(val, rp.PropertyType, out var conv))
			{
				Console.WriteLine($"  -> Could not convert");
				continue;
			}
			Console.WriteLine($"  -> Setting to: '{conv}'");

			rp.SetValue(inst, conv);
		}

		Console.WriteLine($"Final instance after property setting: {inst}");
		return inst;
	}

	private static bool TryGet(IValueProvider provider, object model, string name, out object? value)
	{
		Console.WriteLine($"    TryGet: Looking for '{name}'");

		if (provider.TryGet(name, out value))
		{
			Console.WriteLine($"    TryGet: Found in provider: '{value}'");
			return true;
		}

		// Letzter Versuch: Id o.ä. über Case-Insensitive
		var prop = model.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (prop != null)
		{
			value = prop.GetValue(model);
			Console.WriteLine($"    TryGet: Found in model: '{value}'");
			return true;
		}

		Console.WriteLine($"    TryGet: Not found");
		value = null;
		return false;
	}

	private static bool IsNullable(Type t) => !t.IsValueType || Nullable.GetUnderlyingType(t) != null;

	private static bool TryConvert(object? value, Type targetType, out object? result)
	{
		var nn = Nullable.GetUnderlyingType(targetType) ?? targetType;
		if (value == null) { result = null; return true; }
		if (nn.IsInstanceOfType(value)) { result = value; return true; }

		try
		{
			if (nn.IsEnum)
			{
				if (value is string s) { result = Enum.Parse(nn, s, true); return true; }
				result = Enum.ToObject(nn, Convert.ChangeType(value, Enum.GetUnderlyingType(nn), CultureInfo.InvariantCulture)!);
				return true;
			}
			if (nn == typeof(Guid))
			{
				if (value is Guid g) { result = g; return true; }
				if (value is string sg && Guid.TryParse(sg, out var pg)) { result = pg; return true; }
				result = null; return false;
			}
			if (nn == typeof(DateTime))
			{
				if (value is DateTime dt) { result = dt; return true; }
				if (value is string sd && DateTime.TryParse(sd, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var pdt)) { result = pdt; return true; }
			}
			result = Convert.ChangeType(value, nn, CultureInfo.InvariantCulture);
			return true;
		}
		catch { result = null; return false; }
	}
}

