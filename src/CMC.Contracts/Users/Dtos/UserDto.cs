using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common; // Editor-Attribute + SelectFrom

namespace CMC.Contracts.Users
{
	/// <summary>
	/// Read-DTO für Anzeigen/Antworten (niemals posten).
	/// </summary>
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
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Letzter Login"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime? LastLoginAt { get; set; }

		// FK wird im Editor über Relation-Picker (ExtraField) gesetzt
		[ScaffoldColumn(false)]
		public Guid? CustomerId { get; set; }

		// Anzeige im Grid ok – aber im Editor ausblenden, sobald CustomerId vorhanden ist
		[Display(Name = "Firma")]
		[EditorHideIfExists(nameof(CustomerId))]
		public string? CustomerName { get; set; }

		// Parameterloser Konstruktor für Model Binding
		public UserDto() { }

		// Konstruktor für die Service-Methode mit named parameters
		public UserDto(
			Guid Id,
			string Email,
			string FirstName,
			string LastName,
			string Role,
			string Department,
			bool IsEmailConfirmed,
			DateTime CreatedAt,
			DateTime? LastLoginAt,
			Guid? CustomerId,
			string? CustomerName)
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
		}
	}
}
