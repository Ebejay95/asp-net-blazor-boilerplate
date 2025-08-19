using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Data Transfer Object for User entity external representation.
/// Contains user information for API responses and UI display purposes.
/// Excludes sensitive data like password hashes and reset tokens.
/// </summary>
/// <param name="Id">Unique identifier for the user (hidden from scaffolding)</param>
/// <param name="Email">User's email address used for authentication and communication</param>
/// <param name="FirstName">User's first name for personalization</param>
/// <param name="LastName">User's last name for personalization</param>
/// <param name="IsEmailConfirmed">Indicates whether the user has confirmed their email address</param>
/// <param name="CreatedAt">Timestamp when the user account was created (formatted for German locale)</param>
/// <param name="LastLoginAt">Timestamp of the user's most recent login (null if never logged in)</param>
public record UserDto(
    [property: ScaffoldColumn(false)]
    Guid Id,

    string Email,

    string FirstName,

    string LastName,

    bool IsEmailConfirmed,

    [property: Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    DateTime CreatedAt,

    DateTime? LastLoginAt
);
