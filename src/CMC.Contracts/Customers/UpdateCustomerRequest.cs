using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers;

/// <summary>
/// Request object for updating existing Customer information.
/// Contains all modifiable business fields for customer updates.
/// </summary>
public record UpdateCustomerRequest(
    /// <summary>
    /// Unique identifier of the Customer to update.
    /// </summary>
    [Required]
    Guid Id,

    /// <summary>
    /// Updated company/customer name.
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [Display(Name = "Firmenname")]
    string Name,

    /// <summary>
    /// Updated industry sector.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Display(Name = "Branche")]
    string Industry,

    /// <summary>
    /// Updated number of employees.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Anzahl Mitarbeiter muss 0 oder größer sein")]
    [Display(Name = "Anzahl Mitarbeiter")]
    int EmployeeCount,

    /// <summary>
    /// Updated annual revenue.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Jahresumsatz muss 0 oder größer sein")]
    [Display(Name = "Jahresumsatz")]
    decimal RevenuePerYear,

    /// <summary>
    /// Customer account status. Only administrators can modify this.
    /// </summary>
    [Display(Name = "Aktiv")]
    bool? IsActive = null
);
