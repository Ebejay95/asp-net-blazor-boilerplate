using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Users
{
    public class CreateUserRequest
    {
        [Required, EmailAddress]
        [Display(Name = "E-Mail")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(12)]
        [StrongPassword]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = string.Empty;

        [Required, Display(Name = "Vorname")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Nachname")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100), Display(Name = "Rolle")]
        public string Role { get; set; } = string.Empty;

        [StringLength(100), Display(Name = "Abteilung")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Firma")]
        public Guid? CustomerId { get; set; }
    }
}
