using System;

namespace CMC.Web.Services;

public sealed record EditRevisionItem(
    long Id,
    DateTimeOffset CreatedAt,
    string Action,
    string? UserEmail,
    string Data
);
