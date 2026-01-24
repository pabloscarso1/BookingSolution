using AuthService.Application.Domain;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetAsync(Guid id);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(Guid id);
    }
}
