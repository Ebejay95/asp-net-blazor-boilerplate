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

public static class DialogServiceExtensions
{
    public static void ConfirmDelete(this DialogService dialogs, string title, Func<Task> onConfirm, string? entityTypeName = null)
    {
        var message = string.IsNullOrWhiteSpace(entityTypeName)
            ? "Wirklich löschen?"
            : $"{entityTypeName} wirklich löschen?";

        dialogs.Open(new DialogRequest
        {
            Title = title,
            Message = message,
            ConfirmText = "Löschen",
            OnConfirm = onConfirm,
            CancelText = "Abbrechen",
            OnCancel = () => Task.CompletedTask
        });
    }

    public static void ConfirmDelete(this DialogService dialogs, string title, string message, Func<Task> onConfirm)
    {
        dialogs.Open(new DialogRequest
        {
            Title = title,
            Message = message,
            ConfirmText = "Löschen",
            OnConfirm = onConfirm,
            CancelText = "Abbrechen",
            OnCancel = () => Task.CompletedTask
        });
    }

    public static void ConfirmDeleteWithRelations(this DialogService dialogs, string title, int dependentCount, Func<string, Task> onDecision, string? entityTypeName = null)
    {
        var msg = (dependentCount <= 0)
            ? "Es existieren verknüpfte Daten."
            : $"Es existieren {dependentCount} verknüpfte Einträge.";

        var options = new List<DialogOption>
        {
            new("cascade", "Alles löschen (Kaskade)"),
            new("detach", "Verknüpfungen lösen, dann löschen"),
            new("cancel", "Abbrechen")
        };

        dialogs.Open(new ChoiceRequest
        {
            Title = string.IsNullOrWhiteSpace(entityTypeName) ? title : $"{title} – {entityTypeName}",
            Message = msg,
            MultiSelect = false,
            Options = options,
            OnConfirm = async keys =>
            {
                var k = keys.FirstOrDefault() ?? "cancel";
                if (k == "cancel") return;
                await onDecision(k);
            },
            OnCancel = () => Task.CompletedTask,
            ConfirmText = "Übernehmen",
            CancelText = "Abbrechen"
        });
    }
}
