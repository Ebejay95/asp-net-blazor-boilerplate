using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Request object for removing a user from their customer.
/// </summary>
public record RemoveUserFromCustomerRequest(
    [Required]
    Guid UserId
);
