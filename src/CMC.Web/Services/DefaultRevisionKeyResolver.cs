using System;
using System.Linq;
using System.Reflection;
using CMC.Contracts.Common;

namespace CMC.Web.Services;

public sealed class DefaultRevisionKeyResolver : IRevisionKeyResolver
{
    public bool TryResolve(object model, out string table, out Guid assetId)
    {
        table = "";
        assetId = default;

        if (model is null) return false;
        var t = model.GetType();

        // 1) Attribut [RevisionTable("...")]
        var attr = t.GetCustomAttribute<RevisionTableAttribute>();
        if (attr is not null)
            table = attr.Name;

        // 2) Guid Id suchen (Id oder <TypeName>Id)
        var guidId = TryGetGuidId(model, t, out assetId);

        // 3) Table per einfache Konvention, wenn kein Attribut vorhanden
        if (string.IsNullOrWhiteSpace(table))
            table = InferTableName(t);

        return guidId && !string.IsNullOrWhiteSpace(table);
    }

    private static bool TryGetGuidId(object model, Type t, out Guid id)
    {
        id = default;

        // a) "Id"
        var pId = t.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (pId is not null && pId.PropertyType == typeof(Guid))
        {
            id = (Guid)(pId.GetValue(model) ?? Guid.Empty);
            if (id != Guid.Empty) return true;
        }

        // b) "<BaseName>Id"
        var baseName = BaseName(t.Name);
        var pAlt = t.GetProperty(baseName + "Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (pAlt is not null && pAlt.PropertyType == typeof(Guid))
        {
            id = (Guid)(pAlt.GetValue(model) ?? Guid.Empty);
            return id != Guid.Empty;
        }

        return false;
    }

    private static string InferTableName(Type t)
    {
        var baseName = BaseName(t.Name);
        // naive Pluralisierung: endet auf 'y' -> 'ies', sonst 's'
        if (baseName.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            return baseName.Substring(0, baseName.Length - 1) + "ies";
        return baseName + "s";
    }

    private static string BaseName(string typeName)
    {
        // DTO/VM Suffixe entfernen
        foreach (var suf in new[] { "Dto", "ViewModel", "Vm", "Model" })
        {
            if (typeName.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
                return typeName[..^suf.Length];
        }
        return typeName;
    }
}
