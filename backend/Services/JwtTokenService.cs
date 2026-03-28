using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.DTOs.Auth;
using backend.Models.Entities;
using backend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var accessTokenExpireMinutesString = _configuration["Jwt:AccessTokenExpireMinutes"];

            if (!int.TryParse(accessTokenExpireMinutesString, out var accessTokenExpireMinutes))
            {
                throw new InvalidOperationException("JWT AccessTokenExpireMinutes is invalid");
            }
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenExpireMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(tokenDescriptor);

            return accessToken;
        }
    }
}