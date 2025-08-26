using System;

namespace CMC.Contracts.Common
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class RelationFromAttribute : Attribute
	{
		public string RelationName { get; }
		public bool IsMany { get; }

		public RelationFromAttribute(string relationName, bool isMany = false)
		{
			RelationName = relationName ?? string.Empty;
			IsMany = isMany;
		}
	}
}
