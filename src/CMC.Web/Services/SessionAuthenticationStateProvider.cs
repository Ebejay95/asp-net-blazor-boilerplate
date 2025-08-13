using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CMC.Web.Services;

public class SessionAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionAuthenticationStateProvider> _logger;

    public SessionAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<SessionAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Session == null)
        {
            _logger.LogWarning("❌ Session: HttpContext or Session is null");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        try
        {
            // Session-Daten lesen
            var userId = httpContext.Session.GetString("UserId");
            var userEmail = httpContext.Session.GetString("UserEmail");
            var userFirstName = httpContext.Session.GetString("UserFirstName");
            var userLastName = httpContext.Session.GetString("UserLastName");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userEmail))
            {
                _logger.LogInformation("✅ Session: User authenticated - {Email} (ID: {UserId})", userEmail, userId);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userEmail),
                    new Claim(ClaimTypes.Email, userEmail)
                };

                if (!string.IsNullOrEmpty(userFirstName))
                    claims.Add(new Claim(ClaimTypes.GivenName, userFirstName));

                if (!string.IsNullOrEmpty(userLastName))
                    claims.Add(new Claim(ClaimTypes.Surname, userLastName));

                var identity = new ClaimsIdentity(claims, "Session");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
            }
            else
            {
                _logger.LogInformation("❌ Session: No valid user data found");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Session: Error reading session data");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    public async Task SetUserSessionAsync(string userId, string email, string firstName, string lastName)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
        {
            _logger.LogError("❌ Cannot set session - HttpContext or Session is null");
            return;
        }

        try
        {
            // Session-Daten setzen
            httpContext.Session.SetString("UserId", userId);
            httpContext.Session.SetString("UserEmail", email);
            httpContext.Session.SetString("UserFirstName", firstName);
            httpContext.Session.SetString("UserLastName", lastName);

            // Session explizit speichern
            await httpContext.Session.CommitAsync();

            _logger.LogInformation("✅ Session: User session set - {Email} (ID: {UserId})", email, userId);

            // Authentication State Changed benachrichtigen
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Session: Error setting session data");
        }
    }

    public async Task ClearUserSessionAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
        {
            _logger.LogWarning("❌ Cannot clear session - HttpContext or Session is null");
            return;
        }

        try
        {
            // Alle Session-Daten entfernen
            httpContext.Session.Remove("UserId");
            httpContext.Session.Remove("UserEmail");
            httpContext.Session.Remove("UserFirstName");
            httpContext.Session.Remove("UserLastName");

            // Session explizit speichern
            await httpContext.Session.CommitAsync();

            _logger.LogInformation("🚪 Session: User session cleared");

            // Authentication State Changed benachrichtigen
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Session: Error clearing session data");
        }
    }

    // Debug-Methoden
    public Dictionary<string, string> GetSessionDebugInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var info = new Dictionary<string, string>();

        if (httpContext?.Session != null)
        {
            try
            {
                info["SessionId"] = httpContext.Session.Id;
                info["IsAvailable"] = httpContext.Session.IsAvailable.ToString();
                info["UserId"] = httpContext.Session.GetString("UserId") ?? "null";
                info["UserEmail"] = httpContext.Session.GetString("UserEmail") ?? "null";
                info["UserFirstName"] = httpContext.Session.GetString("UserFirstName") ?? "null";
                info["UserLastName"] = httpContext.Session.GetString("UserLastName") ?? "null";
            }
            catch (Exception ex)
            {
                info["Error"] = ex.Message;
            }
        }
        else
        {
            info["Error"] = "HttpContext or Session is null";
        }

        return info;
    }
}
