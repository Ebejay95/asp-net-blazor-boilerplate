using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CMC.Web.Services
{
    /// <summary>
    /// Wendet Änderungen aus einem Werte-Dictionary (z.B. EditContextAdapter.Values)
    /// generisch auf Relationen an.
    ///
    /// Verhalten:
    ///  - Priorisiert die EF-modellbasierten Deskriptoren (diff auf Join/Collection),
    ///    damit Removals sicher funktionieren, auch wenn Navs nicht geladen sind.
    ///  - Fällt zurück auf Domänenmethode Set{Plural}(IEnumerable&lt;Guid&gt;), falls vorhanden.
    ///
    /// Aufruf im OnSave deiner EFEdit-Dialoge nach Create/Update.
    /// </summary>
    public static class RelationSaveHelper
    {
        /// <param name="rels">IRelationshipManager (DI)</param>
        /// <param name="parentType">z.B. typeof(Customer)</param>
        /// <param name="parentKey">Primärschlüssel des Parents (Guid/String)</param>
        /// <param name="values">Name→Wert (u.a. "IndustryIds" → IEnumerable{string/guid})</param>
        public static async Task ApplyAsync(
            IRelationshipManager rels,
            Type parentType,
            object parentKey,
            IReadOnlyDictionary<string, object?> values)
        {
            if (rels is null) throw new ArgumentNullException(nameof(rels));
            if (parentType is null) throw new ArgumentNullException(nameof(parentType));
            if (parentKey is null) throw new ArgumentNullException(nameof(parentKey));
            if (values is null) return;

            // Parent laden (tracked)
            var parent = await rels.FindParentAsync(parentType, parentKey);

            foreach (var kv in values)
            {
                var name = kv.Key ?? "";
                if (!name.EndsWith("Ids", StringComparison.Ordinal))
                    continue;

                // neue Ziel-Keys normalisieren
                var newKeys = NormalizeKeys(kv.Value);

                // Descriptor robust auflösen: "Industries" -> ggf. "CustomerIndustries"
                var relNameGuess = DeriveRelationName(name, isMany: true);
                var descriptor = TryGetDescriptor(rels, parentType, relNameGuess)
                            ?? TryGetDescriptor(rels, parentType, parentType.Name + relNameGuess);

                if (descriptor == null)
                    throw new InvalidOperationException(
                        $"Relation descriptor not found for '{relNameGuess}' or '{parentType.Name + relNameGuess}' on {parentType.Name}");

                // Aktuelle Keys lesen, Diff bilden
                var current = (await descriptor.LoadCurrentKeys(parent)).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var toAdd = newKeys.Where(k => !current.Contains(k)).ToArray();
                var toRem = current.Where(k => !newKeys.Contains(k, StringComparer.OrdinalIgnoreCase)).ToArray();

                Console.WriteLine($"🧭 [{parentType.Name}] {descriptor.Name}: add={toAdd.Length}, remove={toRem.Length}");

                if (toAdd.Length > 0 && descriptor.AddMany != null)
                    await descriptor.AddMany(parent, toAdd);    // enthält SaveChanges

                if (toRem.Length > 0 && descriptor.RemoveMany != null)
                    await descriptor.RemoveMany(parent, toRem); // enthält SaveChanges
            }
        }

        // --- Helper wie gehabt ---
        private static RelationDescriptor? TryGetDescriptor(IRelationshipManager rels, Type parentType, string relationName)
        {
            try { return rels.GetDescriptor(parentType, relationName); }
            catch { return null; }
        }

        // NormalizeKeys, DeriveRelationName unverändert lassen




        private static bool TryCallDomainSetter(object parent, string idsFieldName, List<string> keys)
        {
            // IndustryIds -> SetIndustries, ScenarioIds -> SetScenarios, TagIds -> SetTags, ...
            var plural = DeriveRelationName(idsFieldName, isMany: true);
            var methodName = "Set" + plural;

            var mi = parent.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (mi == null) return false;

            var p = mi.GetParameters().SingleOrDefault();
            if (p == null) return false;

            if (p.ParameterType == typeof(IEnumerable<Guid>) || p.ParameterType.IsAssignableFrom(typeof(IEnumerable<Guid>)))
            {
                var typed = keys.Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                                .Where(g => g != Guid.Empty);
                mi.Invoke(parent, new object[] { typed });
                return true;
            }

            return false;
        }

        private static List<string> NormalizeKeys(object? v)
        {
            var res = new List<string>();
            if (v is null) return res;

            if (v is IEnumerable<string> ss)
                return ss.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (v is IEnumerable<Guid> gs)
                return gs.Where(g => g != Guid.Empty).Select(g => g.ToString()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (v is System.Collections.IEnumerable seq && v is not string)
            {
                foreach (var e in seq)
                {
                    if (e is null) continue;
                    var s = e.ToString();
                    if (!string.IsNullOrWhiteSpace(s)) res.Add(s!);
                }
                return res.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            var one = v.ToString();
            if (!string.IsNullOrWhiteSpace(one)) res.Add(one!);
            return res;
        }

        /// <summary>Aus "IndustryIds" → "Industries", "TagIds" → "Tags", "ScenarioIds" → "Scenarios" …</summary>
        private static string DeriveRelationName(string idsFieldName, bool isMany)
        {
            var baseName = idsFieldName.EndsWith("Ids", StringComparison.Ordinal)
                ? idsFieldName[..^3]
                : idsFieldName.EndsWith("Id", StringComparison.Ordinal) ? idsFieldName[..^2] : idsFieldName;

            if (!isMany) return baseName;

            return baseName.EndsWith("y", StringComparison.OrdinalIgnoreCase)
                ? baseName[..^1] + "ies"
                : baseName + "s";
        }
    }
}
