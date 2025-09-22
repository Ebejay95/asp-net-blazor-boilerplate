using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Users
{
    /// <summary>Read-DTO für Anzeigen/Antworten (niemals posten).</summary>
    public class UserDto
    {
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Display(Name = "E-Mail")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vorname")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Nachname")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Rolle")]
        [SelectFrom("CMC.Contracts.Users.UserRoles.Roles")]
        public string Role { get; set; } = string.Empty;

        [ScaffoldColumn(false), Display(Name = "Abteilung")]
        [SelectFrom("CMC.Contracts.Users.UserRoles.Departments")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "E-Mail bestätigt")]
        public bool IsEmailConfirmed { get; set; }

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Letzter Login"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset? LastLoginAt { get; set; }

        // 👉 Editor: Relation-Picker (Dropdown). Grid: ausgeblendet.
        [Display(Name = "Firma")]
        [ScaffoldColumn(false)]
        public Guid? CustomerId { get; set; }

        // 👉 Grid: Name anzeigen. Editor: ausblenden.
        [Display(Name = "Firma")]
        [EditorHidden]
        public string? CustomerName { get; set; }

        // 2FA Properties
        [Display(Name = "2FA Secret")]
        [EditorHidden] // Wird in speziellen Formularen angezeigt
        public string? TwoFASecret { get; set; }

        [Display(Name = "2FA aktiviert")]
        public bool TwoFAEnabled { get; set; }

        [Display(Name = "2FA eingerichtet am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset? TwoFAEnabledAt { get; set; }

        public UserDto() { }

        public UserDto(
            Guid Id,
            string Email,
            string FirstName,
            string LastName,
            string Role,
            string Department,
            bool IsEmailConfirmed,
            DateTimeOffset CreatedAt,
            DateTimeOffset? LastLoginAt,
            Guid? CustomerId,
            string? CustomerName,
            string? TwoFASecret = null,
            bool TwoFAEnabled = false,
            DateTimeOffset? TwoFAEnabledAt = null)
        {
            this.Id = Id;
            this.Email = Email;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Role = Role;
            this.Department = Department;
            this.IsEmailConfirmed = IsEmailConfirmed;
            this.CreatedAt = CreatedAt;
            this.LastLoginAt = LastLoginAt;
            this.CustomerId = CustomerId;
            this.CustomerName = CustomerName;
            this.TwoFASecret = TwoFASecret;
            this.TwoFAEnabled = TwoFAEnabled;
            this.TwoFAEnabledAt = TwoFAEnabledAt;
        }
    }
}
