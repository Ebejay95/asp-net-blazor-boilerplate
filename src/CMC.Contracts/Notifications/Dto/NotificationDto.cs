namespace CMC.Contracts.Notifications;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Severity,
    DateTimeOffset CreatedAt,
    bool IsRead
);
