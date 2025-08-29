using System;

namespace CMC.Domain.Entities
{
	public class Control : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// Beziehungen
		public Guid CustomerId { get; private set; }
		public virtual Customer? Customer { get; private set; }

		public Guid LibraryControlId { get; private set; }
		public virtual LibraryControl? LibraryControl { get; private set; }

		// Evidence (optional)
		public Guid? EvidenceId { get; private set; }
		public virtual Evidence? Evidence { get; private set; }

		// Felder aus deiner Tabelle
		public bool Implemented { get; private set; }                 // implemented
		public decimal Coverage { get; private set; }                 // 0..1
		public int Maturity { get; private set; }                     // 0..3 (oder beliebig int)
		public decimal EvidenceWeight { get; private set; }           // 0..1
		public decimal Freshness { get; private set; }                // 0..1
		public decimal CostTotalEur { get; private set; }             // cost_total_eur
		public decimal DeltaEalEur { get; private set; }              // delta_eal_eur
		public decimal Score { get; private set; }                    // frei (z.B. 0..100)
		public string Status { get; private set; } = string.Empty;    // proposed / active / ...
		public DateTimeOffset? DueDate { get; private set; }          // due_date

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft Delete
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
			Status = (status ?? string.Empty).Trim();

			if (evidenceId.HasValue && evidenceId.Value != Guid.Empty) EvidenceId = evidenceId;
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

		public void SetImplemented(bool implemented)
		{
			Implemented = implemented;
			Touch();
		}

		public void SetCoverage(decimal coverage)
		{
			if (coverage < 0m || coverage > 1m) throw new ArgumentOutOfRangeException(nameof(coverage), "Coverage must be between 0 and 1.");
			Coverage = coverage;
			Touch();
		}

		public void SetMaturity(int maturity)
		{
			if (maturity < 0) throw new ArgumentOutOfRangeException(nameof(maturity), "Maturity must be >= 0.");
			Maturity = maturity;
			Touch();
		}

		public void SetEvidenceWeight(decimal evidenceWeight)
		{
			if (evidenceWeight < 0m || evidenceWeight > 1m) throw new ArgumentOutOfRangeException(nameof(evidenceWeight), "EvidenceWeight must be between 0 and 1.");
			EvidenceWeight = evidenceWeight;
			Touch();
		}

		public void SetFreshness(decimal freshness)
		{
			if (freshness < 0m || freshness > 1m) throw new ArgumentOutOfRangeException(nameof(freshness), "Freshness must be between 0 and 1.");
			Freshness = freshness;
			Touch();
		}

		public void SetCosts(decimal totalEur)
		{
			if (totalEur < 0m) throw new ArgumentOutOfRangeException(nameof(totalEur), "CostTotalEur must be >= 0.");
			CostTotalEur = totalEur;
			Touch();
		}

		public void SetDeltaEal(decimal deltaEalEur)
		{
			DeltaEalEur = deltaEalEur;
			Touch();
		}

		public void SetScore(decimal score)
		{
			Score = score;
			Touch();
		}

		public void SetStatus(string? status)
		{
			Status = (status ?? string.Empty).Trim();
			Touch();
		}

		public void SetDueDate(DateTimeOffset? dueDateUtc)
		{
			DueDate = dueDateUtc?.ToUniversalTime();
			Touch();
		}

		public void LinkEvidence(Guid? evidenceId)
		{
			EvidenceId = (evidenceId.HasValue && evidenceId.Value != Guid.Empty) ? evidenceId : null;
			Touch();
		}

		public void ReassignCustomer(Guid customerId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			CustomerId = customerId;
			Touch();
		}

		public void SetLibraryControl(Guid libraryControlId)
		{
			if (libraryControlId == Guid.Empty) throw new ArgumentException("LibraryControlId required.", nameof(libraryControlId));
			LibraryControlId = libraryControlId;
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

        // Prüft, ob die verlinkte Evidence aktuell "gilt" (einfaches Modell)
        // - EvidenceId vorhanden
        // - EvidenceWeight > 0 und Freshness > 0 (Qualitätssignal)
        // - ValidUntil (falls geladen) ist offen oder >= asOfUtc
        public bool HasValidEvidence(DateTimeOffset asOfUtc)
        {
            asOfUtc = asOfUtc.ToUniversalTime();
            if (!EvidenceId.HasValue || EvidenceId.Value == Guid.Empty) return false;
            if (EvidenceWeight <= 0m || Freshness <= 0m) return false;

            var validUntil = Evidence?.ValidUntil; // kann null sein wenn nicht geladen
            if (!validUntil.HasValue) return true; // open-ended
            return validUntil.Value.ToUniversalTime() >= asOfUtc;
        }

        // Setzt DeltaEalEur und berechnet einen 0..100 Score.
        // Heuristik: Nutzen/Kosten (ΔEAL / Kosten) * Maturity-Gewichtung.
        public void RecalculateScoreFromEal(decimal baseEal, decimal residualEal)
        {
            var delta = baseEal - residualEal;
            if (delta < 0m) delta = 0m;

            DeltaEalEur = delta;

            var denom = CostTotalEur <= 0m ? 1m : CostTotalEur;
            var baseScore = 100m * Clamp01(delta / denom);

            // Maturity 0..3 -> 0..1; sanft gewichtet (50% Basis, 50% Maturity)
            var maturityFactor = Clamp01(Maturity / 3m);
            var score = baseScore * (0.5m + 0.5m * maturityFactor);

            Score = Clamp100(score);
            Touch();
        }

        private static decimal Clamp01(decimal v) => v < 0m ? 0m : (v > 1m ? 1m : v);
        private static decimal Clamp100(decimal v) => v < 0m ? 0m : (v > 100m ? 100m : v);

        // Prüft, ob die Bedingungen für "active" erfüllt sind.
        // Wenn Evidence verlinkt ist, muss sie (Stand asOfUtc) gültig sein.
        public bool IsReadyForActivation(DateTimeOffset asOfUtc, out string explanation)
        {
            var issues = new List<string>();

            if (!Implemented) issues.Add("not implemented");
            if (Coverage <= 0m) issues.Add("coverage = 0");
            if (EvidenceWeight <= 0m) issues.Add("evidence_weight = 0");
            if (Freshness <= 0m) issues.Add("freshness = 0");

            if (EvidenceId.HasValue && EvidenceId.Value != Guid.Empty)
            {
                if (!HasValidEvidence(asOfUtc))
                    issues.Add("evidence invalid/expired");
            }

            if (issues.Count == 0)
            {
                explanation = "ready";
                return true;
            }

            explanation = string.Join(", ", issues);
            return false;
        }

        // Darf der Status gewechselt werden? (inkl. Business-Regeln)
        public bool CanTransitionTo(string newStatus, DateTimeOffset asOfUtc, out string? reason)
        {
            reason = null;

            var current = string.IsNullOrWhiteSpace(Status) ? ControlStatus.Proposed : Status.Trim();
            if (!ControlStatus.All.Contains(newStatus))
            {
                reason = $"unknown status '{newStatus}'";
                return false;
            }

            // Strukturregel: Transition erlaubt?
            if (!ControlStatus.AllowedTransitions.TryGetValue(current, out var targets) || !targets.Contains(newStatus))
            {
                reason = $"transition {current} → {newStatus} not allowed";
                return false;
            }

            // Fachregel: Aktivierung braucht Reife / Evidenz
            if (newStatus.Equals(ControlStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                if (!IsReadyForActivation(asOfUtc, out var why))
                {
                    reason = $"not ready for activation: {why}";
                    return false;
                }
            }

            return true;
        }

        // Erzwingt Transition (wirft bei Verstoß)
        public void TransitionTo(string newStatus, DateTimeOffset asOfUtc)
        {
            if (!CanTransitionTo(newStatus, asOfUtc, out var reason))
                throw new InvalidOperationException(reason);

            // Nebenwirkungen bei bestimmten Statuswechseln
            if (newStatus.Equals(ControlStatus.InProgress, StringComparison.OrdinalIgnoreCase) && !Implemented)
            {
                // optional: beim Start noch nicht implementiert
            }
            if (newStatus.Equals(ControlStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                // Aktiv = umgesetzt; Fälligkeitsdatum optional leeren
                if (!Implemented) SetImplemented(true);
                SetDueDate(DueDate); // Touch via SetDueDate; noop, aber behält UTC
            }
            if (newStatus.Equals(ControlStatus.Retired, StringComparison.OrdinalIgnoreCase))
            {
                // optional: beim Retire nichts
            }

            SetStatus(newStatus);
        }
        public void Plan()                       => TransitionTo(ControlStatus.Planned,    DateTimeOffset.UtcNow);
        public void Start()                      => TransitionTo(ControlStatus.InProgress, DateTimeOffset.UtcNow);
        public void Block()                      => TransitionTo(ControlStatus.Blocked,    DateTimeOffset.UtcNow);
        public void Activate(DateTimeOffset now) => TransitionTo(ControlStatus.Active,     now);
        public void Retire()                     => TransitionTo(ControlStatus.Retired,    DateTimeOffset.UtcNow);

	}
}
