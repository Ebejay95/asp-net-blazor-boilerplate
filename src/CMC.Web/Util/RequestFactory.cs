using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CMC.Web.Util;

public static class RequestFactory
{
	public static object MapByProvider(object model, IValueProvider provider, string action, Assembly contractsAssembly)
	{
		var requestType = RequestConventions.FindRequestType(model, action, contractsAssembly)
			?? throw new InvalidOperationException($"Request-Typ für '{action}' nicht gefunden.");
		return MapToRequest(model, provider, requestType);
	}

	private static object MapToRequest(object model, IValueProvider provider, Type requestType)
	{
		var ctors = requestType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
			.OrderByDescending(c => c.GetParameters().Length)
			.ToList();

		foreach (var ctor in ctors)
		{
			var ps = ctor.GetParameters();
			var args = new object?[ps.Length];
			var ok = true;

			for (int i = 0; i < ps.Length; i++)
			{
				var p = ps[i];
				// Fix: Check for null parameter name
				if (string.IsNullOrEmpty(p.Name) || !TryGet(provider, model, p.Name, out var val))
				{
					if (p.HasDefaultValue) { args[i] = p.DefaultValue; continue; }
					if (IsNullable(p.ParameterType)) { args[i] = null; continue; }
					ok = false; break;
				}
				if (!TryConvert(val, p.ParameterType, out var conv)) { ok = false; break; }
				args[i] = conv;
			}

			if (ok) return ctor.Invoke(args);
		}

		// Fallback: parameterlos + setzbare Properties
		var inst = Activator.CreateInstance(requestType) ?? throw new InvalidOperationException($"Cannot create {requestType.Name}");
		var props = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
		foreach (var rp in props)
		{
			// Fix: Check for null property name
			if (string.IsNullOrEmpty(rp.Name) || !TryGet(provider, model, rp.Name, out var val)) continue;
			if (!TryConvert(val, rp.PropertyType, out var conv)) continue;
			rp.SetValue(inst, conv);
		}
		return inst;
	}

	private static bool TryGet(IValueProvider provider, object model, string name, out object? value)
	{
		if (provider.TryGet(name, out value)) return true;

		// Letzter Versuch: Id o.ä. über Case-Insensitive
		var prop = model.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (prop != null) { value = prop.GetValue(model); return true; }

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
