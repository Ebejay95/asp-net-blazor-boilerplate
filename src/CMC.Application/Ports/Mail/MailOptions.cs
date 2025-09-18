namespace CMC.Application.Ports.Mail;

public sealed class MailOptions
{
    public string FromEmail { get; set; } = "no-reply@audicius.de";
    public string FromName  { get; set; } = "CMC";
    public string? PublicBaseUrl { get; set; }

    public SmtpOptions Smtp { get; set; } = new();

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int    Port { get; set; } = 1025;
        public string? Username { get; set; }
        public string? Password { get; set; }
        /// <summary>
        /// true = STARTTLS (typisch Port 587). false = Klartext (z.B. MailHog:1025) oder
        /// Provider mit Implicit TLS (465) NICHT hierüber – dafür lieber MailKit nutzen.
        /// </summary>
        public bool UseStartTls { get; set; } = false;
        public int TimeoutMs { get; set; } = 15000;
    }
}
