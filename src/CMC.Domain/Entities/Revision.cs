using System;

namespace CMC.Domain.Entities;

public class Revision
{
    public long Id { get; set; }
    public string Table { get; set; } = "";                // schema.name
    public Guid AssetId { get; set; }
    public string Action { get; set; } = "Update";         // Create | Update | Delete | Restore
    public string? UserEmail { get; set; }
    public string Data { get; set; } = "";                 // jsonb
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
