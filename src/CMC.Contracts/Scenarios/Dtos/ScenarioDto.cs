using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Scenarios
{
    public class ScenarioDto
    {
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [ScaffoldColumn(false)]
        public Guid CustomerId { get; set; }

        [ScaffoldColumn(false)]
        public Guid LibraryScenarioId { get; set; }

        [Display(Name = "Szenario")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Jährliche Häufigkeit")]
        public decimal AnnualFrequency { get; set; }

        [Display(Name = "Impact (% Umsatz)"), DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal ImpactPctRevenue { get; set; }

        // M:N – IDs für Persistenz
        [Display(Name = "Tags")]
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

        // Optional: nur zur Anzeige
        [Display(Name = "Tag-Namen")]
        public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
