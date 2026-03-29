using backend.Data;
using backend.DTOs.Auth;
using backend.Models.Entities;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(AppDbContext context, 
            IPasswordService passwordService, IJwtTokenService jwtTokenService, 
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<UserProfileDTO> GetMeAsync(int userId)
        {
            var user = await _context.Users!
                .Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            return new UserProfileDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var user = await _context.Users!.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            
            if (user is null)
            {
                throw new Exception("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new Exception("User account is inactive");
            }

            var isPasswordValid = _passwordService.VerifyPassword(
                user, user.PasswordHash, request.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Invalid email or password");
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            var refreshTokeHash = _refreshTokenService.HashToken(refreshToken);
            var refreshTokenExpiresAt = _refreshTokenService.GetRefreshTokenExpiryTime();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokeHash,
                ExpiresAt = refreshTokenExpiresAt
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();


            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserProfile = new UserProfileDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.Name,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                }
            };
        }

        public async Task<UserProfileDTO> RegisterAsync(RegisterRequestDTO request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            // Check if user with the same email already exists
            var emailExists = await _context.Users!.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new Exception("Email is already registered");
            }

            // create new user
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (customerRole is null)
            {
                throw new Exception("Customer role not found");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = normalizedEmail,
                RoleId = customerRole.Id,
                IsActive = true,
            };
            user.PasswordHash = _passwordService.HashPassword(user, request.Password);

            // add to database
            _context.Users!.Add(user);
            await _context.SaveChangesAsync();

            return new UserProfileDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = customerRole.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
            };
        }
    
        public async Task<RefreshResponseDTO> RefreshAsync(string refreshToken)
        {
            var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

            var existingToken = await _context.RefreshTokens!
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

            if (existingToken is null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (existingToken.RevokedAt is not null)
            {
                throw new Exception("Refresh token has been revoked");
            }

            if (existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new Exception("Refresh token has expired");
            }

            var user = existingToken.User;
            if (!user.IsActive)
            {
                throw new Exception("User account is inactive");
            }

            // rotate token
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);
            var newResfreshTokenExpiresAt = _refreshTokenService.GetRefreshTokenExpiryTime();   

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByTokenHash = newRefreshTokenHash;

            var newResfreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = newResfreshTokenExpiresAt
            };
            _context.RefreshTokens.Add(newResfreshTokenEntity);
            await _context.SaveChangesAsync();

            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            return new RefreshResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserProfile = new UserProfileDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.Name,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                }
            };
        }
    
        public async Task LogoutAsync(string refreshToken)
        {
            var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

            var existingToken = await _context.RefreshTokens
                                    .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);
            if (existingToken is null)
            {
                return;
            }

            if (existingToken.RevokedAt is null)
            {
                existingToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}