using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
    public class ToDo : ISoftDeletable
    {
        public Guid Id { get; private set; }

        // Jetzt hart an Control gebunden (Instanz, nicht Library):
        public Guid ControlId { get; private set; }
        public Guid? DependsOnTaskId { get; private set; }

        // Inhalt
        public string Name { get; private set; } = string.Empty;
        public int InternalDays { get; private set; }
        public int ExternalDays { get; private set; }
        public int TotalDays { get; private set; }

        public DateTimeOffset? StartDate { get; private set; }
        public DateTimeOffset? EndDate { get; private set; }

        public ToDoStatus Status { get; private set; } = ToDoStatus.Todo;
        public string Assignee { get; private set; } = string.Empty;

        // Audit
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        // Soft Delete
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        private ToDo() { }

        public ToDo(
            Guid controlId,
            string name,
            int internalDays,
            int externalDays,
            Guid? dependsOnTaskId = null,
            DateTimeOffset? startDateUtc = null,
            DateTimeOffset? endDateUtc = null,
            ToDoStatus status = ToDoStatus.Todo,
            string? assignee = null,
            DateTimeOffset? createdAtUtc = null)
        {
            if (controlId == Guid.Empty) throw new ArgumentException("ControlId required.", nameof(controlId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");

            Id = Guid.NewGuid();
            ControlId = controlId;
            Name = name.Trim();
            InternalDays = internalDays;
            ExternalDays = externalDays;
            TotalDays = internalDays + externalDays;

            if (dependsOnTaskId.HasValue && dependsOnTaskId.Value == Id)
                throw new InvalidOperationException("A task cannot depend on itself.");

            DependsOnTaskId = dependsOnTaskId;
            StartDate = startDateUtc?.ToUniversalTime();
            EndDate = endDateUtc?.ToUniversalTime();
            Status = status;
            Assignee = (assignee ?? string.Empty).Trim();

            CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;

            if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
                throw new ArgumentException("EndDate must be >= StartDate");
        }

        public void SetEffort(int internalDays, int externalDays)
        {
            if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");
            InternalDays = internalDays;
            ExternalDays = externalDays;
            TotalDays = internalDays + externalDays;
            Touch();
        }

        /// <summary>
        /// Einfache Variante (keine DB-Abfragen): setzt die Abhängigkeit.
        /// Schützt nur gegen Self-Dependency; für Zyklus-/Same-Control-Checks siehe Überladung mit Resolvern.
        /// </summary>
        public void DependOn(Guid? taskId)
        {
            if (taskId.HasValue && taskId.Value == Id)
                throw new InvalidOperationException("A task cannot depend on itself.");

            DependsOnTaskId = (taskId.HasValue && taskId.Value != Guid.Empty) ? taskId : null;
            Touch();
        }

        /// <summary>
        /// Setzt die Abhängigkeit mit Zyklus-Check über Resolver.
        /// getParent: liefert die DependsOnTaskId des übergebenen ToDo (oder null/Guid.Empty).
        /// getControlId (optional): liefert die ControlId des übergebenen ToDo (für Same-Control-Check).
        /// requireSameControl: wenn true, muss die Abhängigkeit im selben Control liegen (falls getControlId != null).
        /// maxHops: Sicherheitslimit gegen unendliche Ketten bei inkonsistenten Daten.
        /// </summary>
        public void DependOn(
            Guid? taskId,
            Func<Guid, Guid?> getParent,
            Func<Guid, Guid>? getControlId = null,
            bool requireSameControl = true,
            int maxHops = 1024)
        {
            if (getParent is null) throw new ArgumentNullException(nameof(getParent));

            if (!taskId.HasValue || taskId.Value == Guid.Empty)
            {
                DependsOnTaskId = null;
                Touch();
                return;
            }

            var newDep = taskId.Value;

            if (newDep == Id)
                throw new InvalidOperationException("A task cannot depend on itself.");

            // Optional: gleiche Control erzwingen
            if (requireSameControl && getControlId is not null)
            {
                var depControl = getControlId(newDep);
                if (depControl != ControlId)
                    throw new InvalidOperationException("Dependency must belong to the same Control.");
            }

            // Zyklusprüfung: verfolge Kette newDep -> parent -> parent -> ...
            var visited = new HashSet<Guid>();
            var current = newDep;
            int hops = 0;

            while (true)
            {
                if (!visited.Add(current))
                    throw new InvalidOperationException("Cyclic dependency detected.");

                if (++hops > maxHops)
                    throw new InvalidOperationException("Dependency chain too long or cyclic.");

                var parent = getParent(current);
                if (!parent.HasValue || parent.Value == Guid.Empty)
                    break; // Kette endet → kein Zyklus (der Id einschließt)

                if (parent.Value == Id)
                    throw new InvalidOperationException("Cyclic dependency detected.");

                current = parent.Value;
            }

            DependsOnTaskId = newDep;
            Touch();
        }

        /// <summary>Entfernt die Abhängigkeit.</summary>
        public void ClearDependency()
        {
            if (DependsOnTaskId.HasValue)
            {
                DependsOnTaskId = null;
                Touch();
            }
        }

        public void Schedule(DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
        {
            var start = startDateUtc?.ToUniversalTime();
            var end = endDateUtc?.ToUniversalTime();
            if (start.HasValue && end.HasValue && end < start)
                throw new ArgumentException("EndDate must be >= StartDate");

            StartDate = start;
            EndDate = end;
            Touch();
        }

        public void SetStatus(ToDoStatus status)
        {
            Status = status;
            Touch();
        }

        public void Assign(string? assignee)
        {
            Assignee = (assignee ?? string.Empty).Trim();
            Touch();
        }

        public void MarkDone(DateTimeOffset? finishedAtUtc = null)
        {
            Status = ToDoStatus.Done;
            EndDate = finishedAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
            Touch();
        }

        public void Delete(string? deletedBy = null)
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                DeletedAt = DateTimeOffset.UtcNow;
                DeletedBy = deletedBy;
            }
        }

        public void Restore()
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                DeletedAt = null;
                DeletedBy = null;
                Touch();
            }
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
