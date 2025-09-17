using Microsoft.AspNetCore.Components;

namespace CMC.Web.Pages.FormFields;

public abstract class FormFieldBase : ComponentBase
{
    [Parameter] public string Name { get; set; } = "";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public object? Value { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? Hint { get; set; }
    [Parameter] public EventCallback<object?> OnValueChanged { get; set; }

    // Vom FormRenderer / Edit kontext reingereichte Fehler
    [Parameter] public string[]? Errors { get; set; }

    // Unbekannte Parameter (type, step, autocomplete, …)
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected Task OnChanged(object? v) => OnValueChanged.InvokeAsync(v);

    // Einheitlicher Input-Style + optionales Error-Flag (setzt .is-invalid)
    protected string ControlCss()
        => "form-control" + ((Errors?.Length ?? 0) > 0 ? " is-invalid" : "");
}
