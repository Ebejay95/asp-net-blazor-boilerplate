namespace CMC.Infrastructure.Services;

public sealed class GraphMailOptions
{
    public string TenantId      { get; set; } = default!;
    public string ClientId      { get; set; } = default!;
    public string ClientSecret  { get; set; } = default!;
    public string FromUser      { get; set; } = default!;
    public string? FromEmail    { get; set; }
    public string? FromName     { get; set; }
    public string? PublicBaseUrl { get; set; }
}
