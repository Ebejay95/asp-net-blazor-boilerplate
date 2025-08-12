using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

public class RegisterUserRequest {
  [Required, EmailAddress]
  public string Email {
    get;
    set;
  } = string.Empty;

  [Required, MinLength(8)]
  public string Password {
    get;
    set;
  } = string.Empty;

  [Required]
  public string FirstName {
    get;
    set;
  } = string.Empty;

  [Required]
  public string LastName {
    get;
    set;
  } = string.Empty;
}
