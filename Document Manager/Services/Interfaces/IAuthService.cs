using Document_Manager.DTOs;

using Document_Manager.Models;



namespace Document_Manager.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDtos request);
        Task<TokenResponseDto?> LoginAsync(UserLoginDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(Guid userId);
    }
}
