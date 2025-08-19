using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

/// <summary>
/// Request object for deleting existing user information.
/// </summary>
public record DeleteUserRequest(
	[property: Required]
	Guid Id
);
