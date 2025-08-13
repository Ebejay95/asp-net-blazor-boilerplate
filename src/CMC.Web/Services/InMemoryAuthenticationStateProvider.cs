using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace CMC.Web.Services;

public class InMemoryAuthenticationStateProvider: AuthenticationStateProvider {
  private readonly ILogger<InMemoryAuthenticationStateProvider> _logger;
  private static readonly ConcurrentDictionary<string, UserSession> _userSessions = new();
  private readonly IHttpContextAccessor _httpContextAccessor;

  public InMemoryAuthenticationStateProvider(ILogger<InMemoryAuthenticationStateProvider> logger, IHttpContextAccessor httpContextAccessor) {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public override Task<AuthenticationState> GetAuthenticationStateAsync() {
    try {
      var sessionId = GetSessionId();
      _logger.LogInformation("🔍 Checking authentication for SessionId: {SessionId}", sessionId ?? "null");

      if (!string.IsNullOrEmpty(sessionId) && _userSessions.TryGetValue(sessionId, out var userSession)) {
        _logger.LogInformation("✅ InMemory: User authenticated - {Email}, SessionId: {SessionId}", userSession.Email, sessionId);

        var claims = new[]{
          new Claim(ClaimTypes.NameIdentifier, userSession.UserId),
          new Claim(ClaimTypes.Name, userSession.Email),
          new Claim(ClaimTypes.GivenName, userSession.FirstName),
          new Claim(ClaimTypes.Surname, userSession.LastName),
          new Claim(ClaimTypes.Email, userSession.Email)
        };

        var identity = new ClaimsIdentity(claims, "InMemory");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
      }

      _logger.LogInformation("❌ InMemory: User not authenticated - SessionId: {SessionId}, Available Sessions: {Count}", sessionId ?? "null", _userSessions.Count);

      // Debug: Alle verfügbaren Sessions loggen
      foreach(var kvp in _userSessions) {
        _logger.LogInformation("📋 Available Session: {SessionId} -> {Email}", kvp.Key, kvp.Value.Email);
      }

      return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    } catch (Exception ex) {
      _logger.LogError(ex, "Error in GetAuthenticationStateAsync");
      return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }
  }

  public void SetUserSession(string sessionId, string userId, string email, string firstName, string lastName) {
    var userSession = new UserSession(userId, email, firstName, lastName, DateTime.UtcNow);
    _userSessions[sessionId] = userSession;

    _logger.LogInformation("✅ InMemory: User session set - SessionId: {SessionId}, Email: {Email}", sessionId, email);

    // WICHTIG: Sofort den Authentication State aktualisieren
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  public void ClearUserSession(string sessionId) {
    _userSessions.TryRemove(sessionId, out _);
    _logger.LogInformation("🚪 InMemory: User session cleared - SessionId: {SessionId}", sessionId);

    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  private string? GetSessionId() {
    var httpContext = _httpContextAccessor.HttpContext;

    if (httpContext == null) {
      _logger.LogWarning("⚠️ HttpContext is null");
      return null;
    }

    // Nur aus der aktuellen Session - KEIN Cookie fallback für Blazor Server
    string
      ? sessionId = null;

    try {
      if (httpContext.Session != null) {
        sessionId = httpContext.Session.Id;
        _logger.LogInformation("📋 Session ID from HttpContext.Session: {SessionId}", sessionId);
      }
    } catch (Exception ex) {
      _logger.LogWarning(ex, "Could not access session");
    }

    return sessionId;
  }

  // Debug-Methoden
  public int GetActiveSessionsCount() => _userSessions.Count;

  public IEnumerable<string> GetAllSessionIds() => _userSessions.Keys;

  public Dictionary<string, UserSession> GetAllSessions() => _userSessions.ToDictionary();
}

public record UserSession(string UserId, string Email, string FirstName, string LastName, DateTime CreatedAt);
