namespace CMC.Contracts.RecycleBin;

public sealed record RecycleBinItemDto(
    string Table,
    Guid AssetId,
    string Title,                
    string? Subtitle,
    string? DeletedBy,
    DateTimeOffset DeletedAt,
    string? SnapshotJson
);
