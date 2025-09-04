using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CMC.Application.Services;
using CMC.Contracts.Users;

namespace CMC.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("🔐 API Login attempt for: {Email}", request.Email);

                var user = await _userService.LoginAsync(request);
                if (user is null)
                {
                    _logger.LogWarning("❌ API Login failed for: {Email}", request.Email);
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                await SetAuthenticationCookie(user);

                _logger.LogInformation("✅ API Login successful for: {Email}", request.Email);
                return Ok(new { message = "Login successful", user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ API Login error for: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("✅ API Logout successful");
                return Redirect("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ API Logout error");
                return StatusCode(500, new { message = "Logout failed" });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (HttpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        var user = await _userService.GetByIdAsync(userId);
                        if (user is null) return NotFound();
                        return Ok(user);
                    }
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500);
            }
        }

        private async Task SetAuthenticationCookie(UserDto user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Email),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                    new(ClaimTypes.Surname, user.LastName ?? string.Empty)
                };

                // 🔑 Rolle ins Cookie (ohne dem Cookie „zu vertrauen“ – es ist serverseitig signiert).
                var role = (user.Role ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));                    // z.B. "Super-Admin" oder "User"
                    claims.Add(new Claim(ClaimTypes.Role, role.ToLowerInvariant())); // tolerant für Vergleiche
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProps
                );

                _logger.LogInformation("✅ Auth cookie set for {Email}. Claims: {Claims}",
                    user.Email,
                    string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting authentication cookie");
                throw;
            }
        }
    }
}
