using EbayCloneBuyerService_CoreAPI.DTOs.User;
using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> AuthenticateAsync(LoginRequest loginRequest);
        Task RegisterAsync(RegisterRequest registerRequest);
    }
}
