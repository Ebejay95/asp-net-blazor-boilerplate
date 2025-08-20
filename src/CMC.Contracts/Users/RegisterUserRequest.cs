using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Request object for user registration.
/// Contains all required information for creating a new user account.
/// </summary>
public class RegisterUserRequest
{
    [Required, EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    [Display(Name = "Passwort")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Vorname")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Nachname")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Rolle")]
    public string Role { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Abteilung")]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Firma")]
    public Guid? CustomerId { get; set; }
}
