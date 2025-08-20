using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers;

/// <summary>
/// Request object for deleting existing Customer information.
/// </summary>
public record DeleteCustomerRequest(
    [property: Required]
    Guid Id
);
