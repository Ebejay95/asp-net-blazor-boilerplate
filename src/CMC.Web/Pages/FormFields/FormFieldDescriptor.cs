namespace CMC.Web.Pages.FormFields;

public sealed record FormFieldDescriptor(
    string Name,
    string Label,
    Type FieldType,
    Type? ValueType = null,
    bool ReadOnly = false,
    bool Required = false,
    string? Hint = null,
    object? Value = null,
    Dictionary<string, object>? Parameters = null,
    // Layout
    int? ColXs = 12, int? ColSm = null, int? ColMd = null, int? ColLg = null, int? Order = null
)
{
    public Dictionary<string, object> Parameters { get; } = Parameters ?? new();
}
