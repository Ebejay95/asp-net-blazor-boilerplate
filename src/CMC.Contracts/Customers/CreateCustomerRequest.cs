using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers;

/// <summary>
/// Request object for creating a new Customer (Company).
/// Contains all required business information for customer registration.
/// </summary>
public record CreateCustomerRequest(
    /// <summary>
    /// Company/Customer name for identification.
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [Display(Name = "Firmenname")]
    string Name,

    /// <summary>
    /// Industry sector the customer operates in.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Display(Name = "Branche")]
    string Industry,

    /// <summary>
    /// Number of employees at the customer company.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Anzahl Mitarbeiter muss 0 oder größer sein")]
    [Display(Name = "Anzahl Mitarbeiter")]
    int EmployeeCount,

    /// <summary>
    /// Annual revenue of the customer company.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Jahresumsatz muss 0 oder größer sein")]
    [Display(Name = "Jahresumsatz")]
    decimal RevenuePerYear
);
