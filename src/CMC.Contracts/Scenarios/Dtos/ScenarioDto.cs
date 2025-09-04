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

        // Referenz: Kunde (Dropdown im Editor, GUID-Spalte im Grid ausgeblendet)
        [Display(Name = "Kunde", AutoGenerateField = false)]
        [RelationFrom(IsMany = false, RelationName = "Customer")]
        public Guid CustomerId { get; set; }

        // Nur fürs Grid (lesbarer Name), im Editor ausgeblendet
        [EditorHidden]
        [Display(Name = "Kunde")]
        public string CustomerName { get; set; } = string.Empty;

        // Referenz: Vorlage (Dropdown im Editor, GUID-Spalte im Grid ausgeblendet)
        [Display(Name = "Vorlage", AutoGenerateField = false)]
        [RelationFrom(IsMany = false, RelationName = "LibraryScenario")]
        public Guid LibraryScenarioId { get; set; }

        // Nur fürs Grid (lesbarer Name), im Editor ausgeblendet
        [EditorHidden]
        [Display(Name = "Vorlage")]
        public string LibraryScenarioName { get; set; } = string.Empty;

        [Display(Name = "Szenario")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Jährliche Häufigkeit")]
        public decimal AnnualFrequency { get; set; }

        [Display(Name = "Impact (% Umsatz)"), DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal ImpactPctRevenue { get; set; }

        // M:N – Tags via Join-Collection (Checkboxen im Editor, ID-Spalte im Grid ausblenden)
        [Display(Name = "Tags", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "TagLinks")]
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Tag-Namen")]
        public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
