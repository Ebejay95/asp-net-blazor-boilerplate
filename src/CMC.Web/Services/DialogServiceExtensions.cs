namespace CMC.Web.Services;

public static class DialogServiceExtensions
{
	// Overload, der zu: ConfirmDelete(title, async () => { ... }, entityTypeName) passt
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

	// Klassischer Overload (falls woanders genutzt)
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

	// Passt zu: ConfirmDeleteWithRelations(title, dependentCount, async (strategy) => { ... }, entityTypeName)
	public static void ConfirmDeleteWithRelations(this DialogService dialogs, string title, int dependentCount, Func<string, Task> onDecision, string? entityTypeName = null)
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
