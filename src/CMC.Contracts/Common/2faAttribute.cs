using System;

namespace CMC.Contracts.Common;

/// <summary>
/// Attribute to mark a property as a Two-Factor Authentication secret field.
/// Used by the FormRenderer to generate QR code fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TwoFASecretAttribute : Attribute
{
    /// <summary>
    /// The issuer name to display in the authenticator app.
    /// </summary>
    public string? Issuer { get; set; } = "CMC App";

    /// <summary>
    /// The account name to display in the authenticator app.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Whether to show the secret in plain text (for debugging/setup).
    /// </summary>
    public bool ShowSecret { get; set; } = false;
}

/// <summary>
/// Attribute to mark a property as a Two-Factor Authentication code input field.
/// Used by the FormRenderer to generate 6-digit code input fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TwoFACodeAttribute : Attribute
{
    /// <summary>
    /// Placeholder text for the input field.
    /// </summary>
    public string? Placeholder { get; set; } = "6-stelliger Code";

    /// <summary>
    /// Whether to auto-submit when all 6 digits are entered.
    /// </summary>
    public bool AutoSubmit { get; set; } = false;
}
