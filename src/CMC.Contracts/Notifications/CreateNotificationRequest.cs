using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Notifications;

public record CreateNotificationRequest(
    [property:Required] string Title,
    [property:Required] string Message,
    [property:Required] string Severity,
    Guid? UserId = null,
    Guid? CustomerId = null
);
