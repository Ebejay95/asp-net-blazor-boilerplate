using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Web.Services
{
    // ---------- Models ----------
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

    // ---------- Service ----------
    /// <summary>
    /// Zentrale UI-Dialog-Orchestrierung (Confirm/Choice).
    /// Unterstützt sowohl Event-basierte Öffnung als auch awaitable Helper (ConfirmAsync / ChooseAsync).
    /// </summary>
    public class DialogService
    {
        // UI-Host (DialogHost.razor) abonniert diese Events.
        public event Action<DialogRequest>? OnOpen;
        public event Action<ChoiceRequest>? OnOpenChoice;

        // Legacy: unmittelbares Öffnen über Events
        public void Open(DialogRequest req) => OnOpen?.Invoke(req);
        public void Open(ChoiceRequest req) => OnOpenChoice?.Invoke(req);

        // Neu: Awaitable Bestätigung – garantiert sauberer async Flow im Blazor-SyncContext.
        public Task<bool> ConfirmAsync(
            string title,
            string message,
            string? confirmText = "OK",
            string? cancelText = "Abbrechen")
        {
            // Falls kein Host angemeldet ist, antworte defensiv mit "false".
            if (OnOpen is null)
                return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var req = new DialogRequest
            {
                Title = title,
                Message = message,
                ConfirmText = confirmText ?? "OK",
                CancelText = cancelText,
                OnConfirm = () => { tcs.TrySetResult(true); return Task.CompletedTask; },
                OnCancel = () => { tcs.TrySetResult(false); return Task.CompletedTask; }
            };

            try { OnOpen.Invoke(req); }
            catch { tcs.TrySetResult(false); }

            return tcs.Task;
        }

        /// <summary>
        /// Awaitable Auswahl-Dialog. Gibt die gewählten Keys zurück (leer = Abbruch).
        /// </summary>
        public Task<IReadOnlyList<string>> ChooseAsync(
            string title,
            string message,
            IEnumerable<DialogOption> options,
            bool multiSelect = false,
            string? confirmText = "Übernehmen",
            string? cancelText = "Abbrechen")
        {
            if (OnOpenChoice is null)
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

            var tcs = new TaskCompletionSource<IReadOnlyList<string>>(TaskCreationOptions.RunContinuationsAsynchronously);

            var req = new ChoiceRequest
            {
                Title = title,
                Message = message,
                MultiSelect = multiSelect,
                Options = options?.ToList() ?? new List<DialogOption>(),
                ConfirmText = confirmText,
                CancelText = cancelText,
                OnConfirm = keys => { tcs.TrySetResult(keys ?? Array.Empty<string>()); return Task.CompletedTask; },
                OnCancel = () => { tcs.TrySetResult(Array.Empty<string>()); return Task.CompletedTask; }
            };

            try { OnOpenChoice.Invoke(req); }
            catch { tcs.TrySetResult(Array.Empty<string>()); }

            return tcs.Task;
        }
    }

    // ---------- Convenience Extensions ----------
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Bestätigungsdialog "Löschen" – awaitable.
        /// </summary>
        public static async Task ConfirmDeleteAsync(
            this DialogService dialogs,
            string title,
            Func<Task> onConfirm,
            string? entityTypeName = null)
        {
            var message = string.IsNullOrWhiteSpace(entityTypeName)
                ? "Wirklich löschen?"
                : $"{entityTypeName} wirklich löschen?";

            if (await dialogs.ConfirmAsync(title, message, "Löschen", "Abbrechen"))
                await onConfirm();
        }

        /// <summary>
        /// Bestätigungsdialog "Löschen" mit eigenem Text – awaitable.
        /// </summary>
        public static async Task ConfirmDeleteAsync(
            this DialogService dialogs,
            string title,
            string message,
            Func<Task> onConfirm)
        {
            if (await dialogs.ConfirmAsync(title, message, "Löschen", "Abbrechen"))
                await onConfirm();
        }

        /// <summary>
        /// Wahl zwischen Kaskade/Detach/Abbrechen – awaitable.
        /// </summary>
        public static async Task ConfirmDeleteWithRelationsAsync(
            this DialogService dialogs,
            string title,
            int dependentCount,
            Func<string, Task> onDecision,
            string? entityTypeName = null)
        {
            var msg = (dependentCount <= 0)
                ? "Es existieren verknüpfte Daten."
                : $"Es existieren {dependentCount} verknüpfte Einträge.";

            var options = new List<DialogOption>
            {
                new("cascade", "Alles löschen (Kaskade)"),
                new("detach",  "Verknüpfungen lösen, dann löschen"),
                new("cancel",  "Abbrechen")
            };

            var keys = await dialogs.ChooseAsync(
                string.IsNullOrWhiteSpace(entityTypeName) ? title : $"{title} – {entityTypeName}",
                msg,
                options,
                multiSelect: false,
                confirmText: "Übernehmen",
                cancelText: "Abbrechen");

            var choice = keys.FirstOrDefault() ?? "cancel";
            if (choice is "cascade" or "detach")
                await onDecision(choice);
        }

        // ---- Legacy-Signaturen: delegieren an Async-Varianten, um vorhandenen Code nicht zu brechen ----
        [Obsolete("Use ConfirmDeleteAsync and await it.")]
        public static void ConfirmDelete(this DialogService dialogs, string title, Func<Task> onConfirm, string? entityTypeName = null)
            => _ = dialogs.ConfirmDeleteAsync(title, onConfirm, entityTypeName);

        [Obsolete("Use ConfirmDeleteAsync and await it.")]
        public static void ConfirmDelete(this DialogService dialogs, string title, string message, Func<Task> onConfirm)
            => _ = dialogs.ConfirmDeleteAsync(title, message, onConfirm);

        [Obsolete("Use ConfirmDeleteWithRelationsAsync and await it.")]
        public static void ConfirmDeleteWithRelations(this DialogService dialogs, string title, int dependentCount, Func<string, Task> onDecision, string? entityTypeName = null)
            => _ = dialogs.ConfirmDeleteWithRelationsAsync(title, dependentCount, onDecision, entityTypeName);
    }
}
