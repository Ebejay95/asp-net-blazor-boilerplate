namespace CMC.Web.Shared;

/// <summary>
/// Beschreibt ein zusätzliches Formularfeld (z.B. Relation-Picker oder Passwort),
/// das nicht direkt aus dem DTO gerendert wird.
/// </summary>
public record ExtraField(
	string Name,
	string Label,
	System.Type Type,
	bool ReadOnly = false,
	string? Hint = null,
	string? DataType = null,
	object? Value = null,
	System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>? Options = null,
	System.Func<System.Threading.Tasks.Task<System.Collections.Generic.KeyValuePair<string, string>?>>? OnCreateNew = null,
	System.Func<string, System.Threading.Tasks.Task<System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,string>>>>? OnSearch = null,
	int DebounceMs = 250
);
