using System;
using System.Collections.Generic;

namespace CMC.Contracts.Users
{
	/// <summary>
	/// Zentrale Options-Quelle für Select-Felder (Labels & Tags).
	/// </summary>
	public static class UserOptions
	{
		/// <summary>
		/// Rollen: Label (deutsch) + Tag (englisch, DB-Wert)
		/// </summary>
		public static readonly IReadOnlyList<(string Label, string Tag)> Roles = new (string Label, string Tag)[]
		{
			("Super-Admin", "super-admin"),
			("Admin",       "admin"),
			("User",        "user"),
			("Guest",       "guest"),
		};

		/// <summary>
		/// Abteilungen: Label (deutsch) + Tag (englisch, DB-Wert)
		/// </summary>
		public static readonly IReadOnlyList<(string Label, string Tag)> Departments = new (string Label, string Tag)[]
		{
			("Geschäftsführung", "management"),
			("Leitung",          "executive"),
			("Mitarbeiter",      "staff"),
		};
	}
}
