using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class RememberTokenRepository : IRememberTokenRepository
    {
        private readonly CloneEbayDbContext cloneEbayDbContext;
        public RememberTokenRepository(CloneEbayDbContext cloneEbayDbContext)
        {
            this.cloneEbayDbContext = cloneEbayDbContext;
        }
        public async Task AddRememberTokenAsync(UserRememberToken userRememberToken)
        {
            try
            {
                cloneEbayDbContext.UserRememberTokens.Add(userRememberToken);
                await cloneEbayDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding remember token", ex);
            }
        }

        public async Task DeleteRememberTokenAsync(string tokenHash)
        {
            try
            {
                cloneEbayDbContext.UserRememberTokens.RemoveRange(cloneEbayDbContext.UserRememberTokens.Where(t => t.TokenHash == tokenHash));
                await cloneEbayDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting remember token", ex);
            }
        }

        public async Task<UserRememberToken?> GetRememberTokenByHashAsync(string tokenHash)
        {
            return await cloneEbayDbContext.UserRememberTokens.Include(urt => urt.User)
                .FirstOrDefaultAsync(urt => urt.TokenHash == tokenHash);
        }
    }
}
