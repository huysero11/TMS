namespace backend.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
        string HashToken(string token);
        DateTime GetRefreshTokenExpiryTime();
    }
}