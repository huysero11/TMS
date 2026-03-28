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
                var authResponseDto = await _authService.LoginAsync(request);
                return Ok(new
                {
                    message = "Login successfully!",
                    data = authResponseDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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