using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Request object for assigning a user to a customer.
/// </summary>
public record AssignUserToCustomerRequest(
    [Required]
    Guid UserId,

    [Required]
    Guid CustomerId
);
