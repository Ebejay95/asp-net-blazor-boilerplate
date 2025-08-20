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
/// <param name="Role">User's role within their organization (not scaffolded)</param>
/// <param name="Department">User's department (not scaffolded)</param>
/// <param name="IsEmailConfirmed">Indicates whether the user has confirmed their email address</param>
/// <param name="CreatedAt">Timestamp when the user account was created (formatted for German locale)</param>
/// <param name="LastLoginAt">Timestamp of the user's most recent login (null if never logged in)</param>
/// <param name="CustomerId">ID of the customer this user belongs to (null if not assigned)</param>
/// <param name="CustomerName">Name of the customer/company this user belongs to (null if not assigned)</param>
public record UserDto(
    [property: ScaffoldColumn(false)]
    Guid Id,

    [property: Display(Name = "E-Mail")]
    string Email,

    [property: Display(Name = "Vorname")]
    string FirstName,

    [property: Display(Name = "Nachname")]
    string LastName,

    [property: ScaffoldColumn(false), Display(Name = "Rolle")]
    string Role,

    [property: ScaffoldColumn(false), Display(Name = "Abteilung")]
    string Department,

    [property: Display(Name = "E-Mail bestätigt")]
    bool IsEmailConfirmed,

    [property: Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    DateTime CreatedAt,

    [property: Display(Name = "Letzter Login"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    DateTime? LastLoginAt,

    [property: ScaffoldColumn(false)]
    Guid? CustomerId,

    [property: Display(Name = "Firma")]
    string? CustomerName
);
