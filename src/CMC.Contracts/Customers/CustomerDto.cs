using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers;

public record CustomerDto(
    [property: ScaffoldColumn(false)]
    Guid Id,

    [property: Display(Name = "Firmenname")]
    string Name,

    [property: Display(Name = "Branche")]
    string Industry,

    [property: Display(Name = "Anzahl Mitarbeiter"), DisplayFormat(DataFormatString = "{0:N0}")]
    int EmployeeCount,

    [property: Display(Name = "Jahresumsatz"), DisplayFormat(DataFormatString = "{0:C}")]
    decimal RevenuePerYear,

    [property: Display(Name = "Aktiv")]
    bool IsActive,

    [property: Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    DateTime CreatedAt,

    [property: Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    DateTime UpdatedAt,

    // Nicht im Edit-Form anzeigen:
    [property: ScaffoldColumn(false), Display(Name = "Anzahl Nutzer")]
    int UserCount
);
