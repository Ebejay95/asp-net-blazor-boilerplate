using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Request object for updating existing user information.
/// Contains only the fields that can be modified after user creation.
/// </summary>
public record UpdateUserRequest(
    /// <summary>
    /// Unique identifier of the user to update.
    /// </summary>
    [Required]
    Guid Id,

    /// <summary>
    /// Updated first name for the user.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string FirstName,

    /// <summary>
    /// Updated last name for the user.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string LastName,

    /// <summary>
    /// Email confirmation status. Only administrators can modify this.
    /// </summary>
    bool? IsEmailConfirmed = null
);
