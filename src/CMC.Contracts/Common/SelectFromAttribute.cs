using System;

namespace CMC.Contracts.Common
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class SelectFromAttribute : Attribute
	{
		public string SourcePath { get; }

		/// <summary>
		/// SourcePath z.B. "CMC.Contracts.Users.UserRoles.Roles"
		/// Zeigt auf eine public static Property/Field mit einer Liste von (Label, Tag)
		/// </summary>
		public SelectFromAttribute(string sourcePath)
		{
			SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
		}
	}
}
