namespace CMC.Web.Shared;

/// <summary>
/// Beschreibt ein zusätzliches Formularfeld (z.B. Relation-Picker oder Passwort),
/// das nicht direkt aus dem DTO gerendert wird.
/// </summary>
public record ExtraField(
    string Name,                                     // Property/Request-Feldname (z.B. "CustomerId")
    string Label,                                    // Label im Formular
    System.Type Type,                                // Datentyp (z.B. typeof(Guid?))
    bool ReadOnly = false,                           // read-only?
    string? Hint = null,                             // Hilfetext
    // Spezielle Renderer: "relation-single", "relation-many", "password"
    string? DataType = null,
    object? Value = null,                            // Vorbelegung
    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>? Options = null, // (Text, Value)
    // Callback für "+" im RelationPicker (Inline-Create)
    System.Func<System.Threading.Tasks.Task<System.Collections.Generic.KeyValuePair<string, string>?>>? OnCreateNew = null
);
