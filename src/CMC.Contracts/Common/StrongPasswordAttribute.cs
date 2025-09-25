using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CMC.Contracts.Common{
    /// <summary>
    /// Erzwingt: ≥12 Zeichen, mind. 1 Groß-, 1 Kleinbuchstabe, 1 Ziffer, 1 Sonderzeichen.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class StrongPassword : ValidationAttribute
    {
        private static readonly Regex HasUpper   = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex HasLower   = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex HasDigit   = new(@"\d",    RegexOptions.Compiled);
        private static readonly Regex HasSpecial = new(@"[^A-Za-z0-9]", RegexOptions.Compiled);

        public int MinLength { get; init; } = 12;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var v = value as string ?? string.Empty;

            if (v.Length < MinLength)      return Fail($"Passwort muss mindestens {MinLength} Zeichen lang sein.");
            if (!HasUpper.IsMatch(v))      return Fail("Passwort benötigt mindestens einen Großbuchstaben.");
            if (!HasLower.IsMatch(v))      return Fail("Passwort benötigt mindestens einen Kleinbuchstaben.");
            if (!HasDigit.IsMatch(v))      return Fail("Passwort benötigt mindestens eine Ziffer.");
            if (!HasSpecial.IsMatch(v))    return Fail("Passwort benötigt mindestens ein Sonderzeichen.");

            return ValidationResult.Success;

            ValidationResult Fail(string msg) => new(msg, new[] { validationContext.MemberName! });
        }
    }
}
