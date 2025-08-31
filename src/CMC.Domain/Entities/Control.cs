using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
    public class Control : ISoftDeletable
    {
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "proposed", "planned", "in_progress", "blocked", "active", "retired" };

        private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
        {
            [""] = new(StringComparer.OrdinalIgnoreCase) { "proposed", "planned", "in_progress" },
            ["proposed"] = new(StringComparer.OrdinalIgnoreCase) { "planned", "in_progress", "blocked", "active", "retired" },
            ["planned"] = new(StringComparer.OrdinalIgnoreCase) { "in_progress", "blocked", "retired" },
            ["in_progress"] = new(StringComparer.OrdinalIgnoreCase) { "blocked", "active", "retired" },
            ["blocked"] = new(StringComparer.OrdinalIgnoreCase) { "in_progress", "retired" },
            ["active"] = new(StringComparer.OrdinalIgnoreCase) { "retired" },
            ["retired"] = new(StringComparer.OrdinalIgnoreCase) { }
        };

        private static string Normalize(string? s) => (s ?? string.Empty).Trim();

        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public virtual Customer? Customer { get; private set; }

        public Guid LibraryControlId { get; private set; }
        public virtual LibraryControl? LibraryControl { get; private set; }

        public Guid? EvidenceId { get; private set; }
        public virtual Evidence? Evidence { get; private set; }

        // M:N ↔ Scenario
        public virtual ICollection<ControlScenario> ScenarioLinks { get; private set; } = new List<ControlScenario>();

        // 1:n ↔ ToDo
        public virtual ICollection<ToDo> ToDos { get; private set; } = new List<ToDo>();

        public bool Implemented { get; private set; }
        public decimal Coverage { get; private set; }
        public int Maturity { get; private set; }
        public decimal EvidenceWeight { get; private set; }
        public decimal Freshness { get; private set; }
        public decimal CostTotalEur { get; private set; }
        public decimal DeltaEalEur { get; private set; }
        public decimal Score { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public DateTimeOffset? DueDate { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        private Control() { }

        public Control(
            Guid customerId,
            Guid libraryControlId,
            bool implemented = false,
            decimal coverage = 0m,
            int maturity = 0,
            decimal evidenceWeight = 0m,
            Guid? evidenceId = null,
            decimal freshness = 0m,
            decimal costTotalEur = 0m,
            decimal deltaEalEur = 0m,
            decimal score = 0m,
            string? status = null,
            DateTimeOffset? dueDateUtc = null,
            DateTimeOffset? createdAtUtc = null)
        {
            if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
            if (libraryControlId == Guid.Empty) throw new ArgumentException("LibraryControlId required.", nameof(libraryControlId));

            Id = Guid.NewGuid();
            CustomerId = customerId;
            LibraryControlId = libraryControlId;

            SetImplemented(implemented);
            SetCoverage(coverage);
            SetMaturity(maturity);
            SetEvidenceWeight(evidenceWeight);
            SetFreshness(freshness);

            CostTotalEur = costTotalEur;
            DeltaEalEur = deltaEalEur;
            Score = score;

            Status = Normalize(status).ToLowerInvariant();

            if (evidenceId.HasValue && evidenceId.Value != Guid.Empty)
                EvidenceId = evidenceId;

            DueDate = dueDateUtc?.ToUniversalTime();

            CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Control FromLibrary(
            Guid customerId,
            LibraryControl lib,
            bool implemented = false,
            decimal coverage = 0m,
            int maturity = 0,
            decimal evidenceWeight = 0m,
            Guid? evidenceId = null,
            decimal freshness = 0m,
            decimal costTotalEur = 0m,
            decimal deltaEalEur = 0m,
            decimal score = 0m,
            string? status = null,
            DateTimeOffset? dueDateUtc = null,
            DateTimeOffset? createdAtUtc = null)
        {
            if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
            if (lib == null) throw new ArgumentNullException(nameof(lib));

            return new Control(
                customerId: customerId,
                libraryControlId: lib.Id,
                implemented: implemented,
                coverage: coverage,
                maturity: maturity,
                evidenceWeight: evidenceWeight,
                evidenceId: evidenceId,
                freshness: freshness,
                costTotalEur: costTotalEur,
                deltaEalEur: deltaEalEur,
                score: score,
                status: status,
                dueDateUtc: dueDateUtc,
                createdAtUtc: createdAtUtc
            );
        }

        // ===== Domain-API =====
        public void SetImplemented(bool implemented) { Implemented = implemented; Touch(); }
        public void SetCoverage(decimal coverage)
        {
            if (coverage < 0m || coverage > 1m) throw new ArgumentOutOfRangeException(nameof(coverage));
            Coverage = coverage; Touch();
        }
        public void SetMaturity(int maturity)
        {
            if (maturity < 0) throw new ArgumentOutOfRangeException(nameof(maturity));
            Maturity = maturity; Touch();
        }
        public void SetEvidenceWeight(decimal evidenceWeight)
        {
            if (evidenceWeight < 0m || evidenceWeight > 1m) throw new ArgumentOutOfRangeException(nameof(evidenceWeight));
            EvidenceWeight = evidenceWeight; Touch();
        }
        public void SetFreshness(decimal freshness)
        {
            if (freshness < 0m || freshness > 1m) throw new ArgumentOutOfRangeException(nameof(freshness));
            Freshness = freshness; Touch();
        }
        public void SetCosts(decimal totalEur)
        {
            if (totalEur < 0m) throw new ArgumentOutOfRangeException(nameof(totalEur));
            CostTotalEur = totalEur; Touch();
        }
        public void SetDeltaEal(decimal deltaEalEur) { DeltaEalEur = deltaEalEur; Touch(); }
        public void SetScore(decimal score) { Score = score; Touch(); }
        private void SetStatus(string? status) { Status = Normalize(status).ToLowerInvariant(); Touch(); }
        public void SetDueDate(DateTimeOffset? dueDateUtc) { DueDate = dueDateUtc?.ToUniversalTime(); Touch(); }
        public void LinkEvidence(Guid? evidenceId) { EvidenceId = (evidenceId.HasValue && evidenceId.Value != Guid.Empty) ? evidenceId : null; Touch(); }
        public void ReassignCustomer(Guid customerId)
        {
            if (customerId == Guid.Empty) throw new ArgumentException(nameof(customerId));
            CustomerId = customerId; Touch();
        }
        public void SetLibraryControl(Guid libraryControlId)
        {
            if (libraryControlId == Guid.Empty) throw new ArgumentException(nameof(libraryControlId));
            LibraryControlId = libraryControlId; Touch();
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
                IsDeleted = false; DeletedAt = null; DeletedBy = null; Touch();
            }
        }
        private void ClearStatus() { Status = string.Empty; Touch(); }
        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

        public bool HasValidEvidence(DateTimeOffset asOfUtc)
        {
            asOfUtc = asOfUtc.ToUniversalTime();

            if (!EvidenceId.HasValue || EvidenceId.Value == Guid.Empty) return false;
            if (Evidence is null) return false;
            if (EvidenceWeight <= 0m || Freshness <= 0m) return false;

            var validUntil = Evidence.ValidUntil;
            if (!validUntil.HasValue) return true;
            return validUntil.Value.ToUniversalTime() >= asOfUtc;
        }

        public void RecalculateScoreFromEal(decimal baseEal, decimal residualEal)
        {
            var delta = baseEal - residualEal; if (delta < 0m) delta = 0m;
            DeltaEalEur = delta;
            var denom = CostTotalEur <= 0m ? 1m : CostTotalEur;
            var baseScore = 100m * Clamp01(delta / denom);
            var maturityFactor = Clamp01(Maturity / 3m);
            var score = baseScore * (0.5m + 0.5m * maturityFactor);
            Score = Clamp100(score);
            Touch();
        }
        private static decimal Clamp01(decimal v) => v < 0m ? 0m : (v > 1m ? 1m : v);
        private static decimal Clamp100(decimal v) => v < 0m ? 0m : (v > 100m ? 100m : v);

        public bool IsReadyForActivation(DateTimeOffset asOfUtc, out string explanation)
        {
            var issues = new List<string>();
            if (!Implemented) issues.Add("not implemented");
            if (Coverage <= 0m) issues.Add("coverage = 0");
            if (EvidenceWeight <= 0m) issues.Add("evidence_weight = 0");
            if (Freshness <= 0m) issues.Add("freshness = 0");
            if (EvidenceId.HasValue && EvidenceId.Value != Guid.Empty)
            {
                if (!HasValidEvidence(asOfUtc)) issues.Add("evidence invalid/expired");
            }
            if (issues.Count == 0) { explanation = "ready"; return true; }
            explanation = string.Join(", ", issues); return false;
        }

        public bool CanTransitionTo(string newStatus, DateTimeOffset asOfUtc, out string? reason)
        {
            reason = null;
            var currentNorm = Normalize(Status).ToLowerInvariant();
            var targetNorm = Normalize(newStatus).ToLowerInvariant();

            if (!ValidStatuses.Contains(targetNorm)) { reason = $"unknown status '{newStatus}'"; return false; }
            if (!AllowedTransitions.TryGetValue(currentNorm, out var targets) || !targets.Contains(targetNorm))
            { reason = $"transition {currentNorm} → {targetNorm} not allowed"; return false; }

            if (string.Equals(targetNorm, "active", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsReadyForActivation(asOfUtc, out var why))
                { reason = $"not ready for activation: {why}"; return false; }
            }
            return true;
        }

        public void TransitionTo(string newStatus, DateTimeOffset asOfUtc)
        {
            if (!CanTransitionTo(newStatus, asOfUtc, out var reason))
                throw new InvalidOperationException(reason);
            var targetNorm = Normalize(newStatus).ToLowerInvariant();
            if (targetNorm == "active") { if (!Implemented) SetImplemented(true); }
            SetStatus(targetNorm);
        }

        // ==== M:N Convenience ====

        /// <summary>Ersetzt die Zuordnung auf genau ein Scenario (Kompatibilität zum alten AttachScenario).</summary>
        public void AttachScenario(Guid? scenarioId)
        {
            ScenarioLinks.Clear();
            if (scenarioId.HasValue && scenarioId.Value != Guid.Empty)
                ScenarioLinks.Add(new ControlScenario(Id, scenarioId.Value));
            Touch();
        }

        /// <summary>Ersetzt die Zuordnung auf eine Menge von Scenarios.</summary>
        public void SetScenarios(IEnumerable<Guid>? scenarioIds)
        {
            var target = (scenarioIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).Distinct().ToHashSet();

            var toRemove = ScenarioLinks.Where(l => !target.Contains(l.ScenarioId)).ToList();
            foreach (var r in toRemove) ScenarioLinks.Remove(r);

            var existing = ScenarioLinks.Select(l => l.ScenarioId).ToHashSet();
            foreach (var id in target)
                if (!existing.Contains(id))
                    ScenarioLinks.Add(new ControlScenario(Id, id));

            Touch();
        }

        public IReadOnlyList<Guid> GetScenarioIds() => ScenarioLinks.Select(l => l.ScenarioId).Distinct().ToArray();

        // Convenience
        public void Plan() => TransitionTo("planned", DateTimeOffset.UtcNow);
        public void Start() => TransitionTo("in_progress", DateTimeOffset.UtcNow);
        public void Block() => TransitionTo("blocked", DateTimeOffset.UtcNow);
        public void Activate(DateTimeOffset now) => TransitionTo("active", now.ToUniversalTime());
        public void Retire() => TransitionTo("retired", DateTimeOffset.UtcNow);
    }
}
