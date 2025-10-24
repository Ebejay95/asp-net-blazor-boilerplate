using System.ComponentModel.DataAnnotations;

namespace CMC.Infrastructure.Services;

public sealed class GraphMailOptions
{
    [Required] public string TenantId { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
    [Required] public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Graph Identity, z.B. UPN oder GUID des Senders (UserId).</summary>
    [Required] public string FromUser { get; set; } = string.Empty;

    /// <summary>Anzeigename/ReplyTo (optional).</summary>
    public string? FromName  { get; set; }
    public string? FromEmail { get; set; }

    /// <summary>Basis-URL für Links in Mails (z.B. https://cmc.examplecompany.de)</summary>
    public string? PublicBaseUrl { get; set; }
}
