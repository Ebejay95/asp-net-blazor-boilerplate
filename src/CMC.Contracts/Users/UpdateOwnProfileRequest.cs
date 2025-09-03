// CMC.Contracts.Users/UpdateOwnProfileRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

public class UpdateOwnProfileRequest
{
  [Required, StringLength(100, MinimumLength = 1)]
  public string FirstName { get; set; } = string.Empty;

  [Required, StringLength(100, MinimumLength = 1)]
  public string LastName { get; set; } = string.Empty;

  [StringLength(100)]
  public string? Role { get; set; }

  [StringLength(100)]
  public string? Department { get; set; }
}
