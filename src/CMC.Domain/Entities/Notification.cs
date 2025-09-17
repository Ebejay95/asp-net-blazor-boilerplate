namespace CMC.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid? CustomerId { get; private set; }

    public string Title { get; private set; } = "";
    public string Message { get; private set; } = "";

    public string Severity { get; private set; } = "notice";
    public string Status  { get; private set; } = "unread";

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    public Notification(Guid userId, string title, string message, string severity, Guid? customerId = null)
    {
        UserId = userId; CustomerId = customerId;
        Title = title?.Trim() ?? ""; Message = message?.Trim() ?? "";
        Severity = severity; Status = "unread";
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRead()
    {
        if (Status == "read") return;
        Status = "read"; ReadAt = DateTimeOffset.UtcNow;
    }
}
