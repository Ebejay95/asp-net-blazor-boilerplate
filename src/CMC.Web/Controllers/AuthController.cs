using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using CMC.Application.Services;
using CMC.Contracts.Users;
using CMC.Application.Ports;

namespace CMC.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _cache;

        public AuthController(
            UserService userService,
            IUserRepository userRepository,
            ILogger<AuthController> logger,
            IMemoryCache cache)
        {
            _userService = userService;
            _userRepository = userRepository;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req is null) return BadRequest(new { success = false, message = "Missing payload" });

            try
            {
                _logger.LogInformation("API Login attempt for: {Email}", req.Email);

                var user = await _userService.LoginAsync(req);
                if (user is null)
                {
                    _logger.LogWarning("API Login failed for: {Email}", req.Email);
                    return Unauthorized(new { success = false, message = "Invalid email or password" });
                }

                var freshUserEntity = await _userRepository.GetByEmailAsync(req.Email);
                var has2FA = freshUserEntity != null && !string.IsNullOrWhiteSpace(freshUserEntity.TwoFASecret);

                _logger.LogInformation("Fresh 2FA check for {Email}: Has2FA={Has2FA}, TwoFASecret={HasSecret}",
                    req.Email, has2FA, freshUserEntity?.TwoFASecret != null ? "Present" : "NULL");

                var tx = CreateTx(new TxPayload
                {
                    Email = user.Email,
                    UserId = user.Id,
                    Has2FA = has2FA
                });

                var continueUrl = $"/api/auth/continue?tx={Uri.EscapeDataString(tx)}";

                return Ok(new
                {
                    success = true,
                    requires2FA = has2FA,
                    requiresSetup = !has2FA,
                    message = has2FA ? "2FA verification required" : "Login successful - 2FA setup recommended",
                    redirectUrl = continueUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Login error for: {Email}", req?.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        [HttpGet("continue")]
        public IActionResult Continue([FromQuery] string tx)
        {
            if (!TryTakeTx(tx, out var payload))
                return Redirect("/login?error=session");

            HttpContext.Session.SetString("PendingLogin:Email", payload.Email);
            HttpContext.Session.SetString("PendingLogin:UserId", payload.UserId.ToString());

            var next = payload.Has2FA
                ? $"/verify-2fa?email={Uri.EscapeDataString(payload.Email)}"
                : $"/setup-2fa?email={Uri.EscapeDataString(payload.Email)}";

            _logger.LogInformation("Continue TX for {Email} -> {Next} (Has2FA: {Has2FA})",
                payload.Email, next, payload.Has2FA);
            return Redirect(next);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest req)
        {
            if (req is null) return BadRequest(new { success = false, message = "Missing payload" });

            try
            {
                _logger.LogInformation("2FA verification attempt for: {Email}", req.Email);

                var userEntity = await _userRepository.GetByEmailAsync(req.Email);
                if (userEntity is null)
                {
                    _logger.LogWarning("User not found in DB: {Email}", req.Email);
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                if (string.IsNullOrWhiteSpace(userEntity.TwoFASecret))
                {
                    _logger.LogWarning("User {Email} has no 2FA secret - redirecting to setup", req.Email);
                    return BadRequest(new {
                        success = false,
                        message = "2FA not configured for this user - redirecting to setup",
                        redirectToSetup = true
                    });
                }

                var ok = await _userService.VerifyTOTPCodeAsync(userEntity.TwoFASecret, req.Code);
                if (!ok)
                {
                    _logger.LogWarning("2FA verification failed for: {Email}", req.Email);
                    return Unauthorized(new { success = false, message = "Invalid 2FA code" });
                }

                var tx = CreateTx(new TxPayload
                {
                    Email = userEntity.Email,
                    UserId = userEntity.Id,
                    FinalizeSignIn = true
                });

                var finalizeUrl = $"/api/auth/finalize?tx={Uri.EscapeDataString(tx)}";
                return Ok(new { success = true, message = "2FA verification successful", redirectUrl = finalizeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA verification error for: {Email}", req?.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during 2FA verification" });
            }
        }

        [HttpPost("setup-2fa")]
        public async Task<IActionResult> Setup2FA([FromBody] Setup2FARequest req)
        {
            if (req is null) return BadRequest(new { success = false, message = "Missing payload" });

            try
            {
                _logger.LogInformation("2FA setup attempt for: {Email}", req.Email);

                var userEntity = await _userRepository.GetByEmailAsync(req.Email);
                if (userEntity is null)
                {
                    _logger.LogWarning("User not found in DB: {Email}", req.Email);
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                var isValid = await _userService.VerifyTOTPCodeAsync(req.Secret, req.ConfirmationCode);
                if (!isValid)
                {
                    _logger.LogWarning("2FA setup invalid code for: {Email}", req.Email);
                    return BadRequest(new { success = false, message = "Invalid confirmation code" });
                }

                var success = await _userService.EnableTwoFAAsync(userEntity.Id, req.Secret, req.ConfirmationCode);
                if (!success) return BadRequest(new { success = false, message = "Failed to enable 2FA" });

                var tx = CreateTx(new TxPayload
                {
                    Email = userEntity.Email,
                    UserId = userEntity.Id,
                    FinalizeSignIn = true
                });

                var finalizeUrl = $"/api/auth/finalize?tx={Uri.EscapeDataString(tx)}";
                return Ok(new { success = true, message = "2FA setup successful", redirectUrl = finalizeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA setup error for: {Email}", req?.Email);
                return StatusCode(500, new { success = false, message = "An error occurred during 2FA setup" });
            }
        }

        [HttpGet("finalize")]
        public async Task<IActionResult> Finalize([FromQuery] string tx)
        {
            if (!TryTakeTx(tx, out var payload) || !payload.FinalizeSignIn)
                return Redirect("/login?error=session");

            var userDto = await _userService.GetByIdAsync(payload.UserId);
            if (userDto is null)
            {
                _logger.LogError("User not found for finalize: {UserId}", payload.UserId);
                return Redirect("/login?error=user");
            }

            await SetAuthenticationCookie(userDto);

            HttpContext.Session.Remove("PendingLogin:Email");
            HttpContext.Session.Remove("PendingLogin:UserId");

            _logger.LogInformation("Finalize sign-in for {Email} -> /", payload.Email);
            return Redirect("/");
        }

        [HttpPost("complete-login")]
        public async Task<IActionResult> CompleteLogin([FromBody] CompleteLoginRequest req)
        {
            var email = req?.Email ?? "";
            try
            {
                _logger.LogInformation("Complete login attempt for: {Email}", email);

                var user = await _userService.GetByEmailAsync(email);
                if (user is null) return Unauthorized(new { success = false, message = "User not found" });

                var tx = CreateTx(new TxPayload
                {
                    Email = user.Email,
                    UserId = user.Id,
                    FinalizeSignIn = true
                });

                var finalizeUrl = $"/api/auth/finalize?tx={Uri.EscapeDataString(tx)}";
                return Ok(new { success = true, message = "Login completed successfully", redirectUrl = finalizeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete login error for: {Email}", email);
                return StatusCode(500, new { success = false, message = "An error occurred completing login" });
            }
        }

        [HttpPost("logout")]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                _logger.LogInformation("API Logout successful");
                return Redirect("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Logout error");
                return StatusCode(500, new { success = false, message = "Logout failed" });
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
        public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest req)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

                var user = await _userService.GetByIdAsync(userId);
                if (user is null) return NotFound();

                var isValid = false;
                if (!string.IsNullOrWhiteSpace(req?.CurrentCode) && !string.IsNullOrWhiteSpace(user.TwoFASecret))
                {
                    isValid = await _userService.VerifyTOTPCodeAsync(user.TwoFASecret, req.CurrentCode);
                }

                if (!isValid) return BadRequest(new { success = false, message = "Invalid verification code" });

                await _userService.UpdateTwoFASecretAsync(userId, null);
                _logger.LogInformation("2FA disabled for user: {UserId}", userId);
                return Ok(new { success = true, message = "2FA has been disabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        private record TxPayload
        {
            public string Email { get; init; } = "";
            public Guid UserId { get; init; }
            public bool Has2FA { get; init; }
            public bool FinalizeSignIn { get; init; }
        }

        private string CreateTx(TxPayload payload)
        {
            var key = "auth:tx:" + Guid.NewGuid().ToString("N");
            _cache.Set(key, payload, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return key;
        }

        private bool TryTakeTx(string key, out TxPayload payload)
        {
            if (_cache.TryGetValue(key, out TxPayload? p) && p is not null)
            {
                payload = p;
                _cache.Remove(key);
                return true;
            }
            payload = default!;
            return false;
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
                    new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                    new("TwoFAEnabled", (!string.IsNullOrWhiteSpace(user.TwoFASecret)).ToString()),
                    new("mfa_verified", "true")
                };

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

                _logger.LogInformation("Auth cookie set for {Email}. Claims: {Claims}",
                    user.Email, string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting authentication cookie");
                throw;
            }
        }
    }

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
        public bool TwoFAVerified { get; set; }
    }

    public class Disable2FARequest
    {
        public string CurrentCode { get; set; } = "";
    }
}
