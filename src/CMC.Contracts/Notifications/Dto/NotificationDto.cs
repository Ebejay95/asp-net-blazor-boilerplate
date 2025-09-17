
namespace CMC.Contracts.Notifications.Dtos;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Severity,
    string Status,
    DateTimeOffset CreatedAt
);
