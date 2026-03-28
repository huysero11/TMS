using backend.Models.Entities;

namespace backend.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        // public DateTime ExpriresAtUtc { get; set; }
        public UserProfileDTO UserProfile { get; set; } = null!;
    }
}