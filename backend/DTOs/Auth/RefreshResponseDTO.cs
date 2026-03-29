namespace backend.DTOs.Auth
{
    public class RefreshResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserProfileDTO UserProfile { get; set; } = null!;
    }
}