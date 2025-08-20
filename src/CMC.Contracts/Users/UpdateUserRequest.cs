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
    [Display(Name = "Vorname")]
    string FirstName,

    /// <summary>
    /// Updated last name for the user.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Display(Name = "Nachname")]
    string LastName,

    /// <summary>
    /// Updated role for the user.
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Rolle")]
    string? Role = null,

    /// <summary>
    /// Updated department for the user.
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Abteilung")]
    string? Department = null,

    /// <summary>
    /// Email confirmation status. Only administrators can modify this.
    /// </summary>
    [Display(Name = "E-Mail bestätigt")]
    bool? IsEmailConfirmed = null,

    /// <summary>
    /// Customer assignment. Only administrators can modify this.
    /// </summary>
    [Display(Name = "Firma")]
    Guid? CustomerId = null
);
