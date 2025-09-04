using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers
{
	/// <summary>Delete Customer</summary>
	public record DeleteCustomerRequest([property: Required] Guid Id);
}
