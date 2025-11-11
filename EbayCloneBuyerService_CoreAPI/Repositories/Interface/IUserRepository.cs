using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task RegisterAsync(User user);
    }
}
