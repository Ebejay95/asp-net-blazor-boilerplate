namespace CMC.Web.FormFields;

public sealed record FormFieldDescriptor(
    string Name,
    string Label,
    Type FieldType,
    Type? ValueType = null,
    bool ReadOnly = false,
    bool Required = false,
    string? Hint = null,
    object? Value = null,
    Dictionary<string, object>? Parameters = null
)
{
    public Dictionary<string, object> Parameters { get; } = Parameters ?? new();
}
