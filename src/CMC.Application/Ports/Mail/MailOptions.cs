namespace CMC.Application.Ports.Mail;

public sealed class MailOptions
{
    /// <summary>Absenderadresse (z. B. no-reply@deine-domain.tld)</summary>
    public string FromEmail { get; set; } = "no-reply@yourdomain.tld";

    /// <summary>Anzeigename des Absenders</summary>
    public string FromName  { get; set; } = "CMC";

    /// <summary>Mail-Provider: aktuell nur "Smtp" genutzt</summary>
    public string Provider  { get; set; } = "Smtp";

    /// <summary>Öffentliche Basis-URL der App (für Links in Mails), z. B. https://cmc.audicius.de</summary>
    public string? PublicBaseUrl { get; set; }

    public SmtpOptions Smtp { get; set; } = new();

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 587;
        /// <summary>True = STARTTLS/SSL aktivieren</summary>
        public bool UseStartTls { get; set; } = true;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
