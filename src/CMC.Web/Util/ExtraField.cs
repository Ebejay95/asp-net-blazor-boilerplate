namespace CMC.Web.Util;

public sealed record ExtraField(
    string Name,
    string Label,
    Type Type,
    bool ReadOnly = false,
    string? Hint = null,
    string DataType = "text",
    object? Value = null,
    List<KeyValuePair<string,string>>? Options = null,
    Func<Task<KeyValuePair<string,string>?>>? OnCreateNew = null,
    Func<string, Task<List<KeyValuePair<string,string>>>>? OnSearch = null,
    int DebounceMs = 0,
    Dictionary<string, object>? Parameters = null  // NEW
)
{
    public Dictionary<string, object> Parameters { get; } = Parameters ?? new();
}
