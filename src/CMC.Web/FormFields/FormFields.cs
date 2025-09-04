using Microsoft.AspNetCore.Components;

namespace CMC.Web.FormFields;

public interface IFormField
{
    string Name { get; }
    object? Value { get; set; }
    EventCallback<object?> OnValueChanged { get; set; }
    Dictionary<string, object> AdditionalAttributes { get; }
}
