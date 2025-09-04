namespace CMC.Domain.ValueObjects;

/// <summary>
/// Email value object with validation, normalization and implicit conversions.
/// </summary>
public sealed record Email
{
  public string Value { get; }

  public Email(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Email cannot be empty", nameof(value));

    var normalized = value.Trim();

    if (!IsValidEmail(normalized))
      throw new ArgumentException("Invalid email format", nameof(value));

    Value = normalized.ToLowerInvariant();
  }

  private static bool IsValidEmail(string email)
  {
    try
    {
      var addr = new System.Net.Mail.MailAddress(email);
      return addr.Address == email;
    }
    catch
    {
      return false;
    }
  }

  public static implicit operator string(Email email) => email.Value;
  public static implicit operator Email(string email) => new(email);

  public override string ToString() => Value;
}
