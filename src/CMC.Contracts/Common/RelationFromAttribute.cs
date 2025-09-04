// CMC.Contracts/Common/RelationFromAttribute.cs
using System;

namespace CMC.Contracts.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationFromAttribute : Attribute
    {
        public bool IsMany { get; init; }
        public string? RelationName { get; init; }  // <- Das Property das fehlt

        public RelationFromAttribute(bool isMany = false, string? relationName = null)
        {
            IsMany = isMany;
            RelationName = relationName;
        }
    }
}
