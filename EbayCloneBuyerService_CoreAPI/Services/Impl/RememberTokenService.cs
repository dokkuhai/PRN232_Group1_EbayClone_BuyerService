using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class RememberTokenService : IRememberTokenService
    {
        private readonly IRememberTokenRepository rememberTokenRepository;
        public RememberTokenService(IRememberTokenRepository rememberTokenRepository)
        {
            this.rememberTokenRepository = rememberTokenRepository;
        }
        public async Task AddRememberTokenAsync(int userId, string tokenHash)
        {
            var token = new UserRememberToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            await rememberTokenRepository.AddRememberTokenAsync(token);
        }

        public async Task DeleteRememberTokenAsync(string tokenHash)
        {
            try
            {
                await rememberTokenRepository.DeleteRememberTokenAsync(tokenHash);
            }
            catch (Exception ex)
            {
                throw new ServiceException(ex.Message);
            }
        }

        public async Task<User?> GetRememberTokenByHashAsync(string tokenHash)
        {
            var token =  await rememberTokenRepository.GetRememberTokenByHashAsync(tokenHash);
            return token?.User;
        }
    }
}
