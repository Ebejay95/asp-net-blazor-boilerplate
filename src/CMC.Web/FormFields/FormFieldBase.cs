using Microsoft.AspNetCore.Components;

namespace CMC.Web.FormFields;

public abstract class FormFieldBase : ComponentBase, IFormField
{
    [Parameter] public string Name { get; set; } = "";
    [Parameter] public object? Value { get; set; }
    [Parameter] public EventCallback<object?> OnValueChanged { get; set; }
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string? Hint { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    protected async Task OnChanged(object? newValue)
    {
        Value = newValue;
        if (OnValueChanged.HasDelegate)
            await OnValueChanged.InvokeAsync(newValue);
    }
}
