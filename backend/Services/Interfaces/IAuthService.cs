using backend.DTOs.Auth;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserProfileDTO> RegisterAsync(RegisterRequestDTO request);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<RefreshResponseDTO> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<UserProfileDTO> GetMeAsync(int userId);
    }
}