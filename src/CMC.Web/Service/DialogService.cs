namespace CMC.Web.Services;
/// <summary>
/// Service for managing confirmation dialogs throughout the application.
/// Provides a centralized way to show confirmation prompts for critical actions.
/// </summary>
public class DialogService
{
    /// <summary>
    /// Event triggered when a confirmation dialog should be opened.
    /// </summary>
    public event Action<DialogRequest>? OnOpen;

    /// <summary>
    /// Event triggered when the confirmation dialog should be closed.
    /// </summary>
    public event Action? OnClose;

    /// <summary>
    /// Opens a confirmation dialog with the specified request parameters.
    /// </summary>
    /// <param name="request">Dialog configuration and callbacks</param>
    public void Open(DialogRequest request) => OnOpen?.Invoke(request);

    /// <summary>
    /// Closes the currently open confirmation dialog.
    /// </summary>
    public void Close() => OnClose?.Invoke();

    /// <summary>
    /// Convenience method for delete confirmations with standard messaging.
    /// </summary>
    /// <param name="itemName">Name of the item being deleted</param>
    /// <param name="onConfirm">Action to execute when deletion is confirmed</param>
    /// <param name="itemType">Type of item (default: "item")</param>
    public void ConfirmDelete(string itemName, Func<Task> onConfirm, string itemType = "item")
    {
        Open(new DialogRequest
        {
            Title = $"Delete {itemType}",
            Message = $"Are you sure you want to delete '{itemName}'?",
            DetailMessage = "This action cannot be undone.",
            ConfirmText = "Delete",
            OnConfirm = onConfirm,
            OnCancel  = () =>
            {
                Close();
                return Task.CompletedTask; // einfache No-Op-Task
            }
        });
    }
}

/// <summary>
/// Request object for configuring confirmation dialogs.
/// </summary>
public class DialogRequest
{
    /// <summary>
    /// Dialog title displayed in the header.
    /// </summary>
    public string Title { get; init; } = "Confirm Action";

    /// <summary>
    /// Main confirmation message.
    /// </summary>
    public string Message { get; init; } = "Are you sure you want to proceed?";

    /// <summary>
    /// Optional detailed message for additional context.
    /// </summary>
    public string? DetailMessage { get; init; }

    /// <summary>
    /// Text for the confirmation button.
    /// </summary>
    public string ConfirmText { get; init; } = "Confirm";

    /// <summary>
    /// Text for the cancel button.
    /// </summary>
    public string CancelText { get; init; } = "Cancel";

    /// <summary>
    /// Action to execute when user confirms.
    /// </summary>
	public Func<Task>? OnConfirm { get; init; }

    /// <summary>
    /// Action to execute when user cancels.
    /// </summary>
	public Func<Task>? OnCancel  { get; init; }
}
