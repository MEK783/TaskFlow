using BLFramework.Models;
using BLFramework.Services;
using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/v1.0/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly UserService _userService;
        private readonly InviteService _inviteService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

        private const string RefreshTokenCookieName = "TaskFlowRefreshToken";
        private const string AccessTokenCookieName = "TaskFlowAccessToken";

        public AuthenticationController(
            AuthenticationService authenticationService,
            UserService userService,
            InviteService inviteService,
            RefreshTokenService refreshTokenService,
            ILogger<AuthenticationController> logger,
            IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _inviteService = inviteService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                // Register user (expects SHA512 hash from frontend)
                var createdUser = await _authenticationService.RegisterUserAsync(
                    request.Username,
                    request.Password, // This should be SHA512 hash from frontend
                    request.InviteCode);

                _logger.LogInformation("User {Username} registered successfully", request.Username);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    User = MapUserToDto(createdUser)
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input" });
                }

                // Authenticate user (expects SHA512 hash from frontend)
                var user = await _authenticationService.AuthenticateUserAsync(
                    request.Username,
                    request.Password); // This should be SHA512 hash from frontend

                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Invalid username or password" });
                }

                // Update last login
                await _userService.UpdateLastLoginAsync(user.Id);

                // Generate refresh token
                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString("N"),
                    ExpiresOn = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7),
                    CreatedAt = DateTime.UtcNow
                };

                var createdRefreshToken = await _refreshTokenService.AddAsync(refreshToken);

                // Set refresh token as httpOnly cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = createdRefreshToken.ExpiresOn
                };
                Response.Cookies.Append(RefreshTokenCookieName, createdRefreshToken.Token, cookieOptions);

                _logger.LogInformation("User {Username} logged in successfully", request.Username);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    User = MapUserToDto(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                // Get refresh token from cookie
                if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenValue))
                {
                    // Revoke the refresh token
                    try
                    {
                        await _refreshTokenService.RevokeTokenByStringAsync(refreshTokenValue);
                    }
                    catch
                    {
                        // Token might not exist or already be revoked, that's okay
                    }
                }

                // Clear cookies
                Response.Cookies.Delete(RefreshTokenCookieName);
                Response.Cookies.Delete(AccessTokenCookieName);

                _logger.LogInformation("User logged out successfully");

                return Ok(new { success = true, message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { success = false, message = "An error occurred during logout" });
            }
        }

        [HttpPost("generate-invite")]
        public async Task<IActionResult> GenerateInviteAsync([FromBody] GenerateInviteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input" });
                }

                // Get current user from claims (in a real scenario, extract from JWT)
                // For now, we'll extract from the refresh token cookie
                if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenValue))
                {
                    return Unauthorized(new { success = false, message = "No active session" });
                }

                var refreshToken = await _refreshTokenService.GetByTokenAsync(refreshTokenValue);
                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return Unauthorized(new { success = false, message = "Invalid or expired session" });
                }

                var user = await _userService.GetByIdAsync(refreshToken.UserId);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                // Create new invite
                var expiresOn = DateTime.UtcNow.AddDays(Math.Max(1, Math.Min(365, request.ExpirationDays)));
                var invite = new Invite
                {
                    CreatedById = user.Id,
                    ExpiresOn = expiresOn,
                    CreatedAt = DateTime.UtcNow
                };

                var createdInvite = await _inviteService.AddAsync(invite);

                _logger.LogInformation("Invite generated by user {UserId}", user.Id);

                return Ok(new
                {
                    success = true,
                    message = "Invite code generated successfully",
                    invite = MapInviteToDto(createdInvite)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invite");
                return StatusCode(500, new { success = false, message = "An error occurred while generating invite" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            try
            {
                // Get refresh token from cookie
                if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenValue))
                {
                    return Unauthorized(new { success = false, message = "No active session" });
                }

                var refreshToken = await _refreshTokenService.GetByTokenAsync(refreshTokenValue);
                if (refreshToken == null || !refreshToken.IsActive)
                {
                    Response.Cookies.Delete(RefreshTokenCookieName);
                    return Unauthorized(new { success = false, message = "Session expired, please login again" });
                }

                var user = await _userService.GetByIdAsync(refreshToken.UserId);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { success = false, message = "User not found or inactive" });
                }

                _logger.LogInformation("Token refreshed for user {UserId}", user.Id);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    User = MapUserToDto(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { success = false, message = "An error occurred while refreshing token" });
            }
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt
            };
        }

        private InviteDto MapInviteToDto(Invite invite)
        {
            return new InviteDto
            {
                Id = invite.Id,
                InviteCode = invite.InviteCode,
                CreatedAt = invite.CreatedAt,
                ExpiresOn = invite.ExpiresOn,
                UsedOn = invite.UsedOn,
                IsValid = invite.IsValid,
                IsExpired = invite.IsExpired
            };
        }
    }
}
