using System.Linq;

namespace CMC.Web.Services;

public sealed class RelationDialogService
{
    private readonly DialogService _dialogs;
    private readonly IRelationshipManager _rels;

    public RelationDialogService(DialogService dialogs, IRelationshipManager rels)
    {
        _dialogs = dialogs;
        _rels = rels;
    }

    public async Task OpenForAsync(object parent, string relationName)
    {
        var desc = _rels.GetDescriptor(parent.GetType(), relationName);
        if (desc.Kind == RelationKind.OneToMany)
        {
            _dialogs.Open(new DialogRequest
            {
                Title = relationName,
                Message = "Diese Relation ist 1:n und wird als Liste angezeigt."
            });
            return;
        }

        var options = (await desc.LoadOptions()).Select(o => new DialogOption(o.Value, o.Label)).ToList();
        var current = (await desc.LoadCurrentKeys(parent)).ToHashSet();

        _dialogs.Open(new ChoiceRequest
        {
            Title = relationName,
            Message = desc.Kind == RelationKind.Reference ? "Bitte auswählen" : "Mehrfachauswahl möglich",
            MultiSelect = desc.Kind == RelationKind.ManyToMany,
            Options = options.Select(o => o with { Selected = current.Contains(o.Key) }).ToList(),
            OnConfirm = async keys =>
            {
                if (desc.Kind == RelationKind.Reference && desc.SetReference != null)
                {
                    var key = keys.FirstOrDefault();
                    if (key != null) await desc.SetReference(parent, key);
                    return;
                }

                if (desc.Kind == RelationKind.ManyToMany && desc.AddMany != null && desc.RemoveMany != null)
                {
                    var newSet = keys.ToHashSet();
                    var toAdd = newSet.Except(current).ToArray();
                    var toRem = current.Except(newSet).ToArray();
                    if (toAdd.Length > 0) await desc.AddMany(parent, toAdd);
                    if (toRem.Length > 0) await desc.RemoveMany(parent, toRem);
                }
            }
        });
    }

    public async Task OpenForAsync(Type parentType, object parentKey, string relationName)
    {
        var parent = await _rels.FindParentAsync(parentType, parentKey);
        await OpenForAsync(parent, relationName);
    }
}
