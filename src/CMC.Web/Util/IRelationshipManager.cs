using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMC.Web.Shared;

namespace CMC.Web.Util;

/// <summary>
/// Verwaltet Beziehungen zwischen Entities und deren Side-Effects beim Bearbeiten
/// </summary>
public interface IRelationshipManager
{
    /// <summary>
    /// Registriert eine Beziehung zwischen zwei Entity-Typen
    /// </summary>
    void RegisterRelationship<TSource, TTarget>(
        string relationshipName,
        Func<TSource, Task<IEnumerable<TTarget>>> getRelatedEntities,
        Func<TSource, TTarget, Task>? updateRelation = null,
        Func<TSource, TTarget, Task>? removeRelation = null
    );

    /// <summary>
    /// Führt alle Beziehungs-Updates für eine Entity durch
    /// </summary>
    Task ProcessRelationshipUpdates<T>(T entity, RequestBuildContext context);

    /// <summary>
    /// Holt alle verwandten Entities für eine gegebene Entity
    /// </summary>
    Task<IEnumerable<object>> GetRelatedEntities<T>(T entity, string relationshipName);
}

public class RelationshipManager : IRelationshipManager
{
    private readonly Dictionary<(Type, string), IRelationshipDefinition> _relationships = new();

    public void RegisterRelationship<TSource, TTarget>(
        string relationshipName,
        Func<TSource, Task<IEnumerable<TTarget>>> getRelatedEntities,
        Func<TSource, TTarget, Task>? updateRelation = null,
        Func<TSource, TTarget, Task>? removeRelation = null)
    {
        var key = (typeof(TSource), relationshipName);
        _relationships[key] = new RelationshipDefinition<TSource, TTarget>
        {
            GetRelatedEntities = getRelatedEntities,
            UpdateRelation = updateRelation,
            RemoveRelation = removeRelation
        };
    }

    public async Task ProcessRelationshipUpdates<T>(T entity, RequestBuildContext context)
    {
        var entityType = typeof(T);

        // Finde alle registrierten Beziehungen für diesen Entity-Typ
        foreach (var kvp in _relationships)
        {
            if (kvp.Key.Item1 == entityType)
            {
                var relationship = kvp.Value;
                await relationship.ProcessUpdate(entity, context);
            }
        }
    }

    public async Task<IEnumerable<object>> GetRelatedEntities<T>(T entity, string relationshipName)
    {
        var key = (typeof(T), relationshipName);
        if (_relationships.TryGetValue(key, out var relationship))
        {
            return await relationship.GetRelated(entity);
        }
        return Enumerable.Empty<object>();
    }

    private interface IRelationshipDefinition
    {
        Task ProcessUpdate(object source, RequestBuildContext context);
        Task<IEnumerable<object>> GetRelated(object source);
    }

    private class RelationshipDefinition<TSource, TTarget> : IRelationshipDefinition
    {
        public Func<TSource, Task<IEnumerable<TTarget>>> GetRelatedEntities { get; set; } = default!;
        public Func<TSource, TTarget, Task>? UpdateRelation { get; set; }
        public Func<TSource, TTarget, Task>? RemoveRelation { get; set; }

        public async Task ProcessUpdate(object source, RequestBuildContext context)
        {
            if (source is TSource typedSource)
            {
                var relatedEntities = await GetRelatedEntities(typedSource);

                foreach (var relatedEntity in relatedEntities)
                {
                    // Prüfe ob die Änderungen diese Beziehung betreffen
                    if (ShouldUpdateRelation(context, relatedEntity))
                    {
                        if (UpdateRelation != null)
                            await UpdateRelation(typedSource, relatedEntity);
                    }
                }
            }
        }

        public async Task<IEnumerable<object>> GetRelated(object source)
        {
            if (source is TSource typedSource)
            {
                var related = await GetRelatedEntities(typedSource);
                return related.Cast<object>();
            }
            return Enumerable.Empty<object>();
        }

        private bool ShouldUpdateRelation(RequestBuildContext context, TTarget relatedEntity)
        {
            // Implementiere Logic um zu entscheiden ob eine Relation aktualisiert werden soll
            // Basierend auf den Änderungen im Context
            return true; // Vereinfacht - könnte komplexere Logic enthalten
        }
    }
}
