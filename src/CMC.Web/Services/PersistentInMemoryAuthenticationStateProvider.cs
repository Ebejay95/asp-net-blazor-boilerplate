using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace CMC.Web.Services;

public class PersistentInMemoryAuthenticationStateProvider: AuthenticationStateProvider {
  private readonly ILogger<PersistentInMemoryAuthenticationStateProvider> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  // WICHTIG: Statisches Dictionary - überlebt Blazor-Circuit-Neustart!
  private static readonly ConcurrentDictionary<string, UserSession> _globalUserSessions = new();

  public PersistentInMemoryAuthenticationStateProvider(ILogger<PersistentInMemoryAuthenticationStateProvider> logger, IHttpContextAccessor httpContextAccessor) {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public override Task<AuthenticationState> GetAuthenticationStateAsync() {
    try {
      var sessionId = GetSessionId();
      _logger.LogInformation("🔍 Checking auth for SessionId: {SessionId}", sessionId ?? "null");

      if (!string.IsNullOrEmpty(sessionId) && _globalUserSessions.TryGetValue(sessionId, out var userSession)) {
        _logger.LogInformation("✅ PersistentInMemory: User authenticated - {Email}, SessionId: {SessionId}", userSession.Email, sessionId);

        var claims = new[]{
          new Claim(ClaimTypes.NameIdentifier, userSession.UserId),
          new Claim(ClaimTypes.Name, userSession.Email),
          new Claim(ClaimTypes.GivenName, userSession.FirstName),
          new Claim(ClaimTypes.Surname, userSession.LastName),
          new Claim(ClaimTypes.Email, userSession.Email)
        };

        var identity = new ClaimsIdentity(claims, "PersistentInMemory");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
      }

      _logger.LogInformation("❌ PersistentInMemory: User not authenticated - SessionId: {SessionId}, Available Sessions: {Count}", sessionId ?? "null", _globalUserSessions.Count);

      // Debug: Alle verfügbaren Sessions loggen
      foreach(var kvp in _globalUserSessions.Take(3)) { // Nur erste 3 für Log-Spam-Vermeidung
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
    _globalUserSessions[sessionId] = userSession;

    _logger.LogInformation("✅ PersistentInMemory: User session set - SessionId: {SessionId}, Email: {Email}, Total Sessions: {Count}", sessionId, email, _globalUserSessions.Count);

    // WICHTIG: Sofort den Authentication State aktualisieren
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  public void ClearUserSession(string sessionId) {
    _globalUserSessions.TryRemove(sessionId, out _);
    _logger.LogInformation("🚪 PersistentInMemory: Session cleared - SessionId: {SessionId}, Remaining Sessions: {Count}", sessionId, _globalUserSessions.Count);

    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  public void ClearAllSessions() {
    var count = _globalUserSessions.Count;
    _globalUserSessions.Clear();
    _logger.LogInformation("🧹 PersistentInMemory: All sessions cleared - {Count} sessions removed", count);

    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  private string? GetSessionId() {
    var httpContext = _httpContextAccessor.HttpContext;

    if (httpContext == null) {
      _logger.LogWarning("⚠️ HttpContext is null");
      return null;
    }

    string
      ? sessionId = null;

    try {
      // 1. Aus der Session ID
      if (httpContext.Session != null) {
        sessionId = httpContext.Session.Id;
        _logger.LogInformation("📋 Session ID: {SessionId}", sessionId);
      }
    } catch (Exception ex) {
      _logger.LogWarning(ex, "Could not access session");
    }

    // 2. Falls Session nicht verfügbar, aus Cookie
    if (string.IsNullOrEmpty(sessionId)) {
      sessionId = httpContext.Request.Cookies["CMC_SessionId"];
      _logger.LogInformation("📋 Session ID from Cookie: {SessionId}", sessionId ?? "null");
    }

    return sessionId;
  }

  // Debug-Methoden
  public int GetActiveSessionsCount() => _globalUserSessions.Count;

  public IEnumerable<string> GetAllSessionIds() => _globalUserSessions.Keys;

  public Dictionary<string, UserSession> GetAllSessions() => _globalUserSessions.ToDictionary();

  // Cleanup alte Sessions (optional - für Memory-Management)
  public void CleanupExpiredSessions(TimeSpan maxAge) {
    var cutoff = DateTime.UtcNow.Subtract(maxAge);
    var expiredSessions = _globalUserSessions.Where(kvp => kvp.Value.CreatedAt < cutoff).Select(kvp => kvp.Key).ToList();

    foreach(var sessionId in expiredSessions) {
      _globalUserSessions.TryRemove(sessionId, out _);
    }

    if (expiredSessions.Any()) {
      _logger.LogInformation("🧹 Cleaned up {Count} expired sessions", expiredSessions.Count);
    }
  }
}

public record UserSession(string UserId, string Email, string FirstName, string LastName, DateTime CreatedAt);
