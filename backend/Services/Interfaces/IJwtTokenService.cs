using backend.DTOs.Auth;
using backend.Models.Entities;

namespace backend.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
    }
}