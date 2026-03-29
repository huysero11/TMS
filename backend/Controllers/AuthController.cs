using System.Security.Claims;
using backend.DTOs.Auth;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string RefreshTokenCookieName = "refreshToken";
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userProfile = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    message = "Register successfully!",
                    data = userProfile
                });
            } 
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    message = ex.Message,
                    detail = ex.ToString()

                });
            }
        }
    
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loginResponseDTO = await _authService.LoginAsync(request);
                Response.Cookies.Append(
                    RefreshTokenCookieName,
                    loginResponseDTO.RefreshToken,
                    GetRefreshTokenCookieOptions()
                );
                return Ok(new
                {
                    message = "Login successfully!",
                    data = new AuthResponseDTO
                    {
                        AccessToken = loginResponseDTO.AccessToken,
                        UserProfile = loginResponseDTO.UserProfile
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message,
                    detail = ex.ToString() 
                });
            }
        }

        private CookieOptions GetRefreshTokenCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/api/auth"
            };
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
                {
                    return BadRequest(new { message = "Refresh token is missing" });
                }

                var refreshResponseDTO = await _authService.RefreshAsync(refreshToken);
                Response.Cookies.Append(
                    RefreshTokenCookieName,
                    refreshResponseDTO.RefreshToken,
                    GetRefreshTokenCookieOptions()
                );

                return Ok(new
                {
                    message = "Token refreshed successfully",
                    data = new AuthResponseDTO
                    {
                        AccessToken = refreshResponseDTO.AccessToken,
                        UserProfile = refreshResponseDTO.UserProfile
                    }
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) &&
                    !string.IsNullOrWhiteSpace(refreshToken))
                {
                    await _authService.LogoutAsync(refreshToken);
                }

                Response.Cookies.Delete(RefreshTokenCookieName, GetRefreshTokenCookieOptions());

                return Ok(new
                {
                    message = "Logout successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user id in token"
                });
            }

            var userProfile = await _authService.GetMeAsync(userId);
            if (userProfile is null) {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                message = "User profile retrieved successfully",
                data = userProfile
            });
        }
    }
}