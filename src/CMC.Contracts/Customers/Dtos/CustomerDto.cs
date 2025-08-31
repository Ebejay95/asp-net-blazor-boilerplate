using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common; // Hier liegen RelationFrom/EditorHidden/… Attribute

namespace CMC.Contracts.Customers
{
    public record CustomerDto(
        [property: ScaffoldColumn(false)] Guid Id,

        [property: Display(Name = "Firmenname")] string Name,

        // M:N – dieses Feld wird im Formular als Checkbox-Liste gerendert.
        // NICHT in der Tabelle anzeigen (AutoGenerateField=false).
        [property: Display(Name = "Branchen", AutoGenerateField = false)]
        [property: RelationFrom(IsMany = true, RelationName = "CustomerIndustries")]
        IReadOnlyList<Guid> IndustryIds,

        // Nur für die Tabellenanzeige (lesbare Namen) – im Formular ausblenden.
        [property: Display(Name = "Branchen")]
        [property: EditorHidden]
        IReadOnlyList<string> IndustryNames,

        [property: Display(Name = "Anzahl Mitarbeiter"), DisplayFormat(DataFormatString = "{0:N0}")]
        int EmployeeCount,

        [property: Display(Name = "Jahresumsatz"), DisplayFormat(DataFormatString = "{0:C}")]
        decimal RevenuePerYear,

        [property: Display(Name = "Aktiv")]
        bool IsActive,

        [property: Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        DateTimeOffset CreatedAt,

        [property: Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        DateTimeOffset UpdatedAt,

        [property: ScaffoldColumn(false), Display(Name = "Anzahl Nutzer")]
        int UserCount
    );
}
