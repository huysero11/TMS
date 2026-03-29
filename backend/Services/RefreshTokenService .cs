using System.Security.Cryptography;
using System.Text;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private const int RefreshTokenByteSize = 64;
        private const int RefreshTokenExpiryDays = 7;

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(RefreshTokenByteSize);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetRefreshTokenExpiryTime()
        {
            return DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
        }

        public string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}