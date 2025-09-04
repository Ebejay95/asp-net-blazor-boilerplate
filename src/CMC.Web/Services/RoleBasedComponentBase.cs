using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMC.Web.Services;

public abstract class RoleBasedComponentBase : ComponentBase
{
    [CascadingParameter] private Task<AuthenticationState> AuthTask { get; set; } = default!;
    protected ClaimsPrincipal? User { get; private set; }

    protected bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    protected bool IsSuperAdmin  => User?.IsInRole("super-admin") == true;
    protected bool IsUser        => User?.IsInRole("user") == true;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthTask;
        User = state.User;
    }

    // ---- User-Infos / Claims ----
    protected string? GetClaim(string type, string? fallback = null)
        => User?.FindFirst(type)?.Value ?? fallback;

    protected IEnumerable<string> GetRoles()
        => User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? Enumerable.Empty<string>();

    protected string GetUserId()    => GetClaim(ClaimTypes.NameIdentifier, "—") ?? "—";
    protected string GetUserEmail() => GetClaim(ClaimTypes.Email, "—") ?? "—";
    protected string GetFullName()  => GetClaim("name", User?.Identity?.Name ?? "—")!;

    // ---- Render-Helper ----
    protected static RenderFragment Empty => _ => { };

    protected RenderFragment RenderIf(bool condition, RenderFragment content)
        => condition ? content : Empty;

    protected RenderFragment RenderForAuthenticated(RenderFragment content)
        => RenderIf(IsAuthenticated, content);

    protected RenderFragment RenderForRole(string role, RenderFragment content)
        => RenderIf(User?.IsInRole(role) == true, content);

    protected RenderFragment RenderForSuperAdmin(RenderFragment content)
        => RenderIf(IsSuperAdmin, content);

    protected RenderFragment RenderForUser(RenderFragment content)
        => RenderIf(IsUser, content);

    /// <summary>
    /// Rendert den ersten Treffer (OR-Logik) aus (Rolle, Fragment). Sonst Fallback.
    /// </summary>
    protected RenderFragment RenderConditional(
        IEnumerable<(string role, RenderFragment content)> sections,
        RenderFragment fallback)
        => builder =>
        {
            var roles = new HashSet<string>(GetRoles(), StringComparer.OrdinalIgnoreCase);
            foreach (var (role, frag) in sections)
            {
                if (roles.Contains(role))
                {
                    frag(builder);
                    return;
                }
            }
            fallback(builder);
        };
}
