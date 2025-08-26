namespace CMC.Web.Services;

public sealed record DialogOption(string Key, string Label, bool Selected = false);

public sealed class DialogRequest
{
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public string ConfirmText { get; init; } = "OK";
    public Func<Task>? OnConfirm { get; init; }
    public string? CancelText { get; init; }
    public Func<Task>? OnCancel { get; init; }
}

public sealed class ChoiceRequest
{
    public string Title { get; init; } = "Auswahl";
    public string Message { get; init; } = "";
    public bool MultiSelect { get; init; } = false;
    public IReadOnlyList<DialogOption> Options { get; init; } = Array.Empty<DialogOption>();
    public Func<IReadOnlyList<string>, Task>? OnConfirm { get; init; }
    public Func<Task>? OnCancel { get; init; }
    public string? ConfirmText { get; init; } = "Übernehmen";
    public string? CancelText { get; init; } = "Abbrechen";
}

public class DialogService
{
    public event Action<DialogRequest>? OnOpen;
    public event Action<ChoiceRequest>? OnOpenChoice;

    public void Open(DialogRequest req) => OnOpen?.Invoke(req);
    public void Open(ChoiceRequest req) => OnOpenChoice?.Invoke(req);
}
