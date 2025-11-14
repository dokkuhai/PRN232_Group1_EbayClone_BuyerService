using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IProfileRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserNameAsync(int userId, string newName);
        Task UpdateUserEmailAsync(int userId, string newEmail);
        Task UpdateUserPhoneNumberAsync(int userId, string newPhoneNumber);
        Task UpdateVerifiedEmailStatusAsync(int userId, bool isVerified);
    }
}
