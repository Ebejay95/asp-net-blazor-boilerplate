using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
	public static class ProvisioningService
	{
		public static List<Scenario> MaterializeScenarios(Guid customerId, IEnumerable<LibraryScenario> libScenarios)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (libScenarios == null) throw new ArgumentNullException(nameof(libScenarios));

			return libScenarios
				.Select(lib => Scenario.FromLibrary(customerId, lib))
				.ToList();
		}

		public static List<Control> MaterializeControls(Guid customerId, IEnumerable<LibraryControl> libControls)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (libControls == null) throw new ArgumentNullException(nameof(libControls));

			// Defaults: noch nicht umgesetzt, keine Coverage, etc.
			return libControls
				.Select(lib => Control.FromLibrary(
					customerId: customerId,
					lib: lib,
					implemented: false,
					coverage: 0m,
					maturity: 0,
					evidenceWeight: 0m,
					freshness: 0m,
					costTotalEur: lib.OpexYearEur + lib.CapexEur, // optional Startwert
					deltaEalEur: 0m,
					score: 0m,
					status: "proposed"
				))
				.ToList();
		}
	}
}
