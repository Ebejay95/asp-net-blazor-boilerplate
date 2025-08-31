using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Scenarios
{
    public class ScenarioDto
    {
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        // 👉 direkt auswählbar im Formular
        [Display(Name = "Kunde")]
        public Guid CustomerId { get; set; }

        // 👉 Vorlage auswählbar
        [Display(Name = "Vorlage")]
        public Guid LibraryScenarioId { get; set; }

        [Display(Name = "Szenario")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Jährliche Häufigkeit")]
        public decimal AnnualFrequency { get; set; }

        [Display(Name = "Impact (% Umsatz)"), DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal ImpactPctRevenue { get; set; }

        // 👉 M:N – IDs editierbar
        [Display(Name = "Tags")]
        [RelationFrom(IsMany = true)]
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

        // 👉 nur Anzeige im Grid
        [EditorHidden]
        [Display(Name = "Tag-Namen")]
        public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
