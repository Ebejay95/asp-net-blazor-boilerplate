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
		// Debug-Ausgabe hinzufügen
		Console.WriteLine($"CompositeValueProvider.TryGet: Searching for '{name}'");

		// Erst in Changes schauen (höhere Priorität)
		if (_changes.TryGetValue(name, out value))
		{
			Console.WriteLine($"Found '{name}' in changes: '{value}'");
			return true;
		}

		// Dann im Model schauen (mit case-insensitive Suche)
		var prop = _model.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (prop != null)
		{
			value = prop.GetValue(_model);
			Console.WriteLine($"Found '{name}' in model: '{value}'");
			return true;
		}

		// Debug: Alle verfügbaren Keys ausgeben
		Console.WriteLine($"Available changes keys: {string.Join(", ", _changes.Keys)}");
		Console.WriteLine($"Available model properties: {string.Join(", ", _model.GetType().GetProperties().Select(p => p.Name))}");

		value = null;
		Console.WriteLine($"'{name}' not found anywhere");
		return false;
	}

	// Hilfsmethode für Debugging
	public void DebugPrint()
	{
		Console.WriteLine("=== CompositeValueProvider Debug ===");
		Console.WriteLine("Changes:");
		foreach (var kvp in _changes)
		{
			Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
		}

		Console.WriteLine("Model properties:");
		foreach (var prop in _model.GetType().GetProperties())
		{
			try
			{
				var val = prop.GetValue(_model);
				Console.WriteLine($"  {prop.Name} = {val}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  {prop.Name} = ERROR: {ex.Message}");
			}
		}
	}
}
