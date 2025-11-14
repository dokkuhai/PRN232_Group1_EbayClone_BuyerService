using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IRememberTokenRepository
    {
        Task AddRememberTokenAsync(UserRememberToken userRememberToken);
        Task<UserRememberToken?> GetRememberTokenByHashAsync(string tokenHash);
        Task DeleteRememberTokenAsync(string tokenHash);
    }
}
