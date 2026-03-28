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

        public AuthService(AppDbContext context, 
            IPasswordService passwordService, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
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

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
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

            var token = _jwtTokenService.GenerateAccessToken(user);
            return new AuthResponseDTO
            {
                AccessToken = token,
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
    }
}