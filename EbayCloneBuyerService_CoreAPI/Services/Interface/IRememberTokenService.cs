using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IRememberTokenService
    {
        Task AddRememberTokenAsync(int userId, string tokenHash);
        Task<User?> GetRememberTokenByHashAsync(string tokenHash);
        Task DeleteRememberTokenAsync(string tokenHash);
    }
}
