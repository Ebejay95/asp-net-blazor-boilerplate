using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Users
{
	/// <summary>
	/// Edit-DTO für Create/Update-Operationen (Form/Command).
	/// </summary>
	public class UserEditDto
	{
		[Required]
		public Guid Id { get; set; }

		[Required, EmailAddress, MaxLength(255)]
		[Display(Name = "E-Mail")]
		public string Email { get; set; } = string.Empty;

		[Required, MaxLength(100)]
		[Display(Name = "Vorname")]
		public string FirstName { get; set; } = string.Empty;

		[Required, MaxLength(100)]
		[Display(Name = "Nachname")]
		public string LastName { get; set; } = string.Empty;

		[MaxLength(100)]
		[Display(Name = "Rolle")]
		[SelectFrom("CMC.Contracts.Users.UserOptions.Roles")]
		public string Role { get; set; } = string.Empty;

		[MaxLength(100)]
		[Display(Name = "Abteilung")]
		[SelectFrom("CMC.Contracts.Users.UserOptions.Departments")]
		public string Department { get; set; } = string.Empty;

		// 👉 Relation: Einzelauswahl über RelationPicker, ohne Inline-Create
		[Display(Name = "Firma")]
		[RelationFrom("Customer", isMany: false)]
		public Guid? CustomerId { get; set; }
	}
}
