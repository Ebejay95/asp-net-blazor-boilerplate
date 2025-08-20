// src/CMC.Web/Services/Relations.cs
namespace CMC.Web.Services;

public enum RelationKind
{
    Reference,   // n:1 oder 1:1 -> Single Select
    OneToMany,   // 1:n -> meist read-only Liste (ableitbar)
    ManyToMany   // n:m -> Multi Select
}

public sealed record RelationOption(string Label, string Value);

public sealed class RelationDescriptor
{
    public required string Name { get; init; }
    public required RelationKind Kind { get; init; }
    public required string FieldKey { get; init; }

    public required Func<Task<IEnumerable<RelationOption>>> LoadOptions { get; init; }
    public required Func<object, Task<object?>> LoadCurrentSelection { get; init; }

    // Reference
    public Func<object, string?, Task>? SetReference { get; init; }

    // ManyToMany
    public Func<object, IEnumerable<string>, Task>? AddMany { get; init; }
    public Func<object, IEnumerable<string>, Task>? RemoveMany { get; init; }

    public Func<object, Task>? OnChanged { get; init; }
}
