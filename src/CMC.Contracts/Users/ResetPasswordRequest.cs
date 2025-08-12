using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Users;

public class ResetPasswordRequest {
  [Required]
  public string Token {
    get;
    set;
  } = string.Empty;

  [Required, MinLength(8)]
  public string NewPassword {
    get;
    set;
  } = string.Empty;
}
