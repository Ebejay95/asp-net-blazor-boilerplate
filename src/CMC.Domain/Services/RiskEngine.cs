using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
    /// <summary>
    /// Einfache Risiko-Engine ohne Link-Property-abhängigkeiten.
    /// Standard-Logik:
    ///   baseEAL = frequency * (impactPct * revenue)
    ///   pro implementiertem Control:
    ///       w = Coverage * EvidenceWeight * Freshness * MaturityFactor
    ///       residualFrequency *= (1 - freqEffect * w)
    ///       residualImpactPct  *= (1 - impactEffect * w)
    ///
    /// Default: freqEffect = 1.0, impactEffect = 1.0 (gleich starke Wirkung auf beide Dimensionen).
    /// Per Delegate kann man (freqEffect, impactEffect) je Control/Szenario überschreiben.
    /// </summary>
    public static class RiskEngine
    {
        /// <summary>
        /// Basis: EAL = AnnualFrequency * (ImpactPctRevenue * RevenuePerYear)
        /// </summary>
        public static decimal ComputeBaseEal(Scenario scenario, decimal revenuePerYear)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (revenuePerYear < 0m) throw new ArgumentOutOfRangeException(nameof(revenuePerYear));

            var freq = ClampNonNegative(scenario.AnnualFrequency);
            var impPct = Clamp01(scenario.ImpactPctRevenue);

            return freq * (impPct * revenuePerYear);
        }

        /// <summary>
        /// Wendet Control-Wirkungen an – ohne gespeicherte Link-Properties.
        /// Du kannst optional einen Effect-Provider übergeben, der je Control (freqEffect, impactEffect) zurückgibt (0..1).
        /// Wenn kein Provider angegeben ist, wird (1,1) verwendet.
        /// </summary>
        public static (decimal residualFrequency, decimal residualImpactPct) ApplyControlEffects(
            Scenario scenario,
            IEnumerable<Control> controlsAffectingScenario,
            Func<Control, Scenario, (decimal freqEffect, decimal impactEffect)>? effectProvider = null)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (controlsAffectingScenario == null) throw new ArgumentNullException(nameof(controlsAffectingScenario));

            decimal freq = ClampNonNegative(scenario.AnnualFrequency);
            decimal impPct = Clamp01(scenario.ImpactPctRevenue);

            var provider = effectProvider ?? DefaultEffectProvider;

            foreach (var control in controlsAffectingScenario)
            {
                if (control == null) continue;
                if (!control.Implemented) continue;

                // Qualitätsgewicht des Controls
                var coverage = Clamp01(control.Coverage);
                var evidence = Clamp01(control.EvidenceWeight);
                var freshness = Clamp01(control.Freshness);
                var maturityFactor = Clamp01(control.Maturity / 3m); // 0..3 → 0..1

                var w = coverage * evidence * freshness * maturityFactor;
                if (w <= 0m) continue;

                var (freqEff, impactEff) = provider(control, scenario);
                freqEff = Clamp01(freqEff);
                impactEff = Clamp01(impactEff);

                // multiplicative Reduktion, nie negativ
                freq *= (1m - freqEff * w);
                impPct *= (1m - impactEff * w);
            }

            // sanitäre Klammern
            return (ClampNonNegative(freq), Clamp01(impPct));
        }

        /// <summary>
        /// Residual-EAL mit optionalem Effect-Provider (siehe ApplyControlEffects).
        /// </summary>
        public static decimal ComputeResidualEal(
            Scenario scenario,
            IEnumerable<Control> controlsAffectingScenario,
            decimal revenuePerYear,
            Func<Control, Scenario, (decimal freqEffect, decimal impactEffect)>? effectProvider = null)
        {
            var (resFreq, resImpPct) = ApplyControlEffects(scenario, controlsAffectingScenario, effectProvider);
            return resFreq * (resImpPct * revenuePerYear);
        }

        /// <summary>
        /// Delta-EAL = Base - Residual (niemals negativ).
        /// </summary>
        public static decimal ComputeDeltaEal(decimal baseEal, decimal residualEal)
        {
            var delta = baseEal - residualEal;
            return delta < 0m ? 0m : delta;
        }

        /// <summary>
        /// Default: Control wirkt gleich stark auf Frequency und Impact (1.0 / 1.0).
        /// </summary>
        private static (decimal freqEffect, decimal impactEffect) DefaultEffectProvider(Control c, Scenario s)
            => (1m, 1m);

        private static decimal Clamp01(decimal v) => v < 0m ? 0m : (v > 1m ? 1m : v);
        private static decimal ClampNonNegative(decimal v) => v < 0m ? 0m : v;
    }
}
