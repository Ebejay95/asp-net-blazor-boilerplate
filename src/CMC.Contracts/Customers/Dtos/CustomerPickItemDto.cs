using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers;

/// <summary>
/// Schlankes DTO für Picker/Dropdowns: nur das, was für die Auswahl benötigt wird.
/// </summary>
public record CustomerPickItemDto(
    [property: ScaffoldColumn(false)]
    Guid Id,

    [property: Display(Name = "Firmenname")]
    string Name
);
