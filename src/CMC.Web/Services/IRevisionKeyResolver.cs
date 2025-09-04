using System;

namespace CMC.Web.Services;

/// <summary>
/// Ermittelt Table-Name und Asset-Id aus dem aktuellen Model, ohne Entity-spezifisches UI.
/// </summary>
public interface IRevisionKeyResolver
{
    /// <summary>Gibt true zurück, wenn eine (Table, Id) für das Model ermittelt werden konnte.</summary>
    bool TryResolve(object model, out string table, out Guid assetId);
}
