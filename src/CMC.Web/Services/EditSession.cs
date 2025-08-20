using CMC.Web.Shared;
using CMC.Web.Util;
using System.Reflection;

namespace CMC.Web.Services;

public sealed class EditSession
{
    public object Model { get; }
    public Assembly ContractsAssembly { get; }
    public string Action { get; }
    public List<RelationDescriptor> Relations { get; } = new();
    public List<ExtraField> ExtraFields { get; } = new();
    private readonly Dictionary<string, object?> _accumulatedChanges = new(StringComparer.OrdinalIgnoreCase);

    public EditSession(object model, Assembly contracts, string action = "Update")
    {
        Model = model;
        ContractsAssembly = contracts;
        Action = action;
    }

    public void ApplyLocalChanges(IReadOnlyDictionary<string, object?> changes)
    {
        foreach (var kv in changes) _accumulatedChanges[kv.Key] = kv.Value;
    }

    public IValueProvider BuildProvider()
        => new CompositeValueProvider(Model, _accumulatedChanges);

    public RequestBuildContext BuildCtx()
        => new RequestBuildContext(Model, ContractsAssembly, BuildProvider(), Action);

}
