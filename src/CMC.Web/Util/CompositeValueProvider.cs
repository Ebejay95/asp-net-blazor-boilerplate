using System;
using System.Collections.Generic;
using System.Reflection;

namespace CMC.Web.Util;

public sealed class CompositeValueProvider : IValueProvider
{
	private readonly object _model;
	private readonly IReadOnlyDictionary<string, object?> _changes;

	public CompositeValueProvider(object model, IReadOnlyDictionary<string, object?> changes)
	{
		_model = model;
		_changes = changes;
	}

	public bool TryGet(string name, out object? value)
	{
		if (_changes.TryGetValue(name, out value)) return true;

		var prop = _model.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (prop != null) { value = prop.GetValue(_model); return true; }

		value = null;
		return false;
	}
}
