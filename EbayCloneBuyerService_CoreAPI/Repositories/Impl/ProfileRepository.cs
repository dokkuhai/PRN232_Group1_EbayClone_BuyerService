using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly CloneEbayDbContext _context;
        public ProfileRepository(CloneEbayDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FindAsync(userId);
        }

        public async Task UpdateUserEmailAsync(int userId, string newEmail)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.Email = newEmail;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateUserNameAsync(int userId, string newName)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.Username = newName;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateUserPhoneNumberAsync(int userId, string newPhoneNumber)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.PhoneNumber = newPhoneNumber;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateVerifiedEmailStatusAsync(int userId, bool isVerified)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.IsEmailVerified = isVerified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
