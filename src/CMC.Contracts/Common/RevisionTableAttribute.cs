using System;

namespace CMC.Contracts.Common;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RevisionTableAttribute : Attribute
{
    public string Name { get; }
    public RevisionTableAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
}
