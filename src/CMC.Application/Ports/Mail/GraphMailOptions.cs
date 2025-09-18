namespace CMC.Application.Ports.Mail;

public sealed class GraphMailOptions
{
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    /// <summary>
    /// UPN oder GUID des Absender-Postfachs (z. B. shared mailbox).
    /// Wird für /users/{id|upn}/sendMail verwendet.
    /// </summary>
    public string? FromUser { get; set; }

    public string? PublicBaseUrl { get; set; }

    // Nur Anzeigezwecke in Templates (optional)
    public string? FromEmail { get; set; }
    public string? FromName  { get; set; }
}
