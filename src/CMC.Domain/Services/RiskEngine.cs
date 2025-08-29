using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
	public static class RiskEngine
	{
		// Basis: EAL = AnnualFrequency * (ImpactPctRevenue * RevenuePerYear)
		public static decimal ComputeBaseEal(Scenario scenario, decimal revenuePerYear)
		{
			if (scenario == null) throw new ArgumentNullException(nameof(scenario));
			if (revenuePerYear < 0m) throw new ArgumentOutOfRangeException(nameof(revenuePerYear));
			return scenario.AnnualFrequency * (scenario.ImpactPctRevenue * revenuePerYear);
		}

		// Residual: wende Control-Effekte an (pro Link freq/impact-Effekt ∈ [0..1])
		// Effektgewichtung über Control-Attribute: Coverage, EvidenceWeight, Freshness, optional Maturity-Faktor
		public static (decimal residualFrequency, decimal residualImpactPct) ApplyControlEffects(
			LibraryScenario libScenario,
			IEnumerable<(Control control, LibraryControlScenario link)> controlLinksForScenario)
		{
			if (libScenario == null) throw new ArgumentNullException(nameof(libScenario));
			if (controlLinksForScenario == null) throw new ArgumentNullException(nameof(controlLinksForScenario));

			decimal freq = libScenario.AnnualFrequency;
			decimal impPct = libScenario.ImpactPctRevenue;

			foreach (var (control, link) in controlLinksForScenario)
			{
				if (control == null || link == null) continue;
				if (!control.Implemented) continue;

				// Weight aus Control-Qualität
				var weight = Clamp01(control.Coverage) * Clamp01(control.EvidenceWeight) * Clamp01(control.Freshness);
				// optional: maturity light-weight (0..3 → 0.0 .. 1.0)
				var maturityFactor = Math.Min(1m, Math.Max(0m, control.Maturity / 3m));
				var w = weight * maturityFactor;

				// Multiplikatives Reduktionsmodell: f_res = f * (1 - eff*w)
				freq *= (1m - Clamp01(link.FrequencyEffect) * w);
				impPct *= (1m - Clamp01(link.ImpactEffect) * w);
			}

			return (freq, impPct);
		}

		public static decimal ComputeResidualEal(
			Scenario scenario,
			LibraryScenario libScenario,
			IEnumerable<(Control control, LibraryControlScenario link)> controlLinksForScenario,
			decimal revenuePerYear)
		{
			var (resFreq, resImpPct) = ApplyControlEffects(libScenario, controlLinksForScenario);
			return resFreq * (resImpPct * revenuePerYear);
		}

		public static decimal ComputeDeltaEal(decimal baseEal, decimal residualEal) => baseEal - residualEal;

		private static decimal Clamp01(decimal v) => v < 0m ? 0m : (v > 1m ? 1m : v);
	}
}
