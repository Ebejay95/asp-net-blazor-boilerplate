using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CMC.Application.Services;
using Microsoft.Extensions.Logging;

namespace CMC.Web.Services;

public sealed class DbBackedClaimsTransformation : IClaimsTransformation
{
    private readonly UserService _users;
    private readonly ILogger<DbBackedClaimsTransformation> _logger;

    public DbBackedClaimsTransformation(UserService users, ILogger<DbBackedClaimsTransformation> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
        {
            _logger.LogDebug("ClaimsTransformation: No valid NameIdentifier in principal.");
            return principal;
        }

        var user = await _users.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("ClaimsTransformation: User {UserId} not found.", userId);
            return principal;
        }

        var ident = (ClaimsIdentity)principal.Identity!;

        // alte Role-Claims entfernen
        foreach (var rc in ident.FindAll(ident.RoleClaimType).ToList())
            ident.RemoveClaim(rc);

        // frische Rolle aus DB setzen
        var role = (user.Role ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(role))
        {
            ident.AddClaim(new Claim(ident.RoleClaimType, role));
            ident.AddClaim(new Claim(ident.RoleClaimType, role.ToLowerInvariant()));
        }

        _logger.LogInformation("ClaimsTransformation: Refreshed claims for {Email}: {Claims}",
            user.Email,
            string.Join(", ", ident.Claims.Select(c => $"{c.Type}={c.Value}")));

        return principal;
    }
}
