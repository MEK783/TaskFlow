using BLFramework.Models;
using BLFramework.Services;
using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Authentication controller for handling user registration, login, logout, and session management.
    /// Manages refresh tokens via HTTP-only cookies for secure session handling.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
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

        /// <summary>
        /// Registers a new user with the system using an invite code.
        /// The password should be the SHA512 hash from the frontend for double-hashing security.
        /// </summary>
        /// <param name="request">The registration request containing username, password hash, and invite code.</param>
        /// <returns>
        /// An HTTP 200 OK response with the created user details if successful,
        /// or HTTP 400 Bad Request if validation fails or invite code is invalid.
        /// </returns>
        [HttpPost("register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Authenticates a user and establishes a session via refresh token cookie.
        /// The password should be the SHA512 hash from the frontend for double-hashing security.
        /// </summary>
        /// <param name="request">The login request containing username, password hash, and remember me preference.</param>
        /// <returns>
        /// An HTTP 200 OK response with the authenticated user details and refresh token cookie if successful,
        /// or HTTP 401 Unauthorized if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Logs out the current user by invalidating their refresh token and clearing cookies.
        /// </summary>
        /// <returns>
        /// An HTTP 200 OK response indicating successful logout,
        /// or HTTP 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpPost("logout")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Generates a new invite code for inviting new users to the system.
        /// Requires an active authenticated session.
        /// </summary>
        /// <param name="request">The request containing the expiration days for the invite code.</param>
        /// <returns>
        /// An HTTP 200 OK response with the generated invite code if successful,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPost("generate-invite")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Refreshes the authentication session using the current refresh token.
        /// Returns updated user information without requiring login credentials again.
        /// </summary>
        /// <returns>
        /// An HTTP 200 OK response with updated user information if successful,
        /// or HTTP 401 Unauthorized if the refresh token is invalid or expired.
        /// </returns>
        [HttpPost("refresh")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Maps a User entity to a UserDto for API responses.
        /// </summary>
        /// <param name="user">The user entity to map.</param>
        /// <returns>A UserDto containing the mapped user information.</returns>
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

        /// <summary>
        /// Maps an Invite entity to an InviteDto for API responses.
        /// </summary>
        /// <param name="invite">The invite entity to map.</param>
        /// <returns>An InviteDto containing the mapped invite information.</returns>
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
