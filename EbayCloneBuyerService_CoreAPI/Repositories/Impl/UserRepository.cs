using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly CloneEbayDbContext _context;
        public UserRepository(CloneEbayDbContext context)
        {
            _context = context;
        }
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
           return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task RegisterAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
