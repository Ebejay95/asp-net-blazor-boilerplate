namespace CMC.Application.Ports.Mail;

public sealed class MailOptions
{
    public string? FromEmail { get; set; }
    public string? FromName  { get; set; }
    public string? PublicBaseUrl { get; set; }

    public SmtpOptions Smtp { get; } = new();

    public sealed class SmtpOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseStartTls { get; set; } = true;
    }
}
