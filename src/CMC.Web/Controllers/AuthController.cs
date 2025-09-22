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

                // Prüfe 2FA Status
                var requires2FA = !string.IsNullOrWhiteSpace(user.TwoFASecret);

                if (requires2FA)
                {
                    // 2FA erforderlich - noch kein vollständiger Login
                    _logger.LogInformation("🔐 2FA required for: {Email}", request.Email);
                    return Ok(new {
                        success = true,
                        requires2FA = true,
                        message = "2FA verification required",
                        userId = user.Id
                    });
                }
                else
                {
                    // Kein 2FA - sofortiger Login oder Setup erforderlich
                    await SetAuthenticationCookie(user);
                    _logger.LogInformation("✅ API Login successful for: {Email} (no 2FA)", request.Email);
                    return Ok(new {
                        success = true,
                        requires2FA = false,
                        requiresSetup = true,
                        message = "Login successful - 2FA setup recommended",
                        user
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ API Login error for: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request)
        {
            try
            {
                _logger.LogInformation("🔐 2FA verification attempt for: {Email}", request.Email);

                var user = await _userService.GetByEmailAsync(request.Email);
                if (user is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                if (string.IsNullOrWhiteSpace(user.TwoFASecret))
                {
                    return BadRequest(new { message = "2FA not configured for this user" });
                }

                var isValidCode = await _userService.VerifyTOTPCodeAsync(user.TwoFASecret, request.Code);

                if (isValidCode)
                {
                    await SetAuthenticationCookie(user);
                    _logger.LogInformation("✅ 2FA verification successful for: {Email}", request.Email);
                    return Ok(new { success = true, message = "2FA verification successful", user });
                }
                else
                {
                    _logger.LogWarning("❌ 2FA verification failed for: {Email}", request.Email);
                    return Unauthorized(new { message = "Invalid 2FA code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 2FA verification error for: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during 2FA verification" });
            }
        }

        [HttpPost("setup-2fa")]
        public async Task<IActionResult> Setup2FA([FromBody] Setup2FARequest request)
        {
            try
            {
                _logger.LogInformation("🔐 2FA setup attempt for: {Email}", request.Email);

                var user = await _userService.GetByEmailAsync(request.Email);
                if (user is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Verifiziere den Setup-Code
                var isValidCode = await _userService.VerifyTOTPCodeAsync(request.Secret, request.ConfirmationCode);

                if (isValidCode)
                {
                    // Speichere das 2FA Secret
                    await _userService.UpdateTwoFASecretAsync(user.Id, request.Secret);

                    // Setze Auth Cookie
                    await SetAuthenticationCookie(user);

                    _logger.LogInformation("✅ 2FA setup successful for: {Email}", request.Email);
                    return Ok(new { success = true, message = "2FA setup successful" });
                }
                else
                {
                    _logger.LogWarning("❌ 2FA setup failed - invalid code for: {Email}", request.Email);
                    return BadRequest(new { message = "Invalid confirmation code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 2FA setup error for: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during 2FA setup" });
            }
        }

        [HttpPost("complete-login")]
        public async Task<IActionResult> CompleteLogin([FromBody] CompleteLoginRequest request)
        {
            try
            {
                _logger.LogInformation("🔐 Complete login attempt for: {Email}", request.Email);

                var user = await _userService.GetByEmailAsync(request.Email);
                if (user is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Zusätzliche Validierung falls nötig
                if (request.TwoFAVerified || string.IsNullOrWhiteSpace(user.TwoFASecret))
                {
                    await SetAuthenticationCookie(user);
                    _logger.LogInformation("✅ Complete login successful for: {Email}", request.Email);
                    return Ok(new { success = true, message = "Login completed successfully", user });
                }
                else
                {
                    return Unauthorized(new { message = "2FA verification required" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Complete login error for: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred completing login" });
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

        [HttpPost("disable-2fa")]
        [Authorize]
        public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest request)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user is null)
                {
                    return NotFound();
                }

                // Verifiziere Passwort oder aktuellen 2FA Code
                var isValid = false;
                if (!string.IsNullOrWhiteSpace(request.CurrentCode) && !string.IsNullOrWhiteSpace(user.TwoFASecret))
                {
                    isValid = await _userService.VerifyTOTPCodeAsync(user.TwoFASecret, request.CurrentCode);
                }

                if (isValid)
                {
                    await _userService.UpdateTwoFASecretAsync(userId, null); // Remove 2FA
                    _logger.LogInformation("✅ 2FA disabled for user: {UserId}", userId);
                    return Ok(new { success = true, message = "2FA has been disabled" });
                }
                else
                {
                    return BadRequest(new { message = "Invalid verification code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, new { message = "An error occurred" });
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

                // 2FA Status als Claim hinzufügen
                var has2FA = !string.IsNullOrWhiteSpace(user.TwoFASecret);
                claims.Add(new Claim("TwoFAEnabled", has2FA.ToString()));

                // Rolle ins Cookie
                var role = (user.Role ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    claims.Add(new Claim(ClaimTypes.Role, role.ToLowerInvariant()));
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

    // Request DTOs
    public class Verify2FARequest
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
    }

    public class Setup2FARequest
    {
        public string Email { get; set; } = "";
        public string Secret { get; set; } = "";
        public string ConfirmationCode { get; set; } = "";
    }

    public class CompleteLoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool TwoFAVerified { get; set; }
    }

    public class Disable2FARequest
    {
        public string CurrentCode { get; set; } = "";
    }
}
