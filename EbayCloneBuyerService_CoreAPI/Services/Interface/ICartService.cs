using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface ICartService
    {
        Task<IEnumerable<UserCart>> GetUserCart(string token);
        Task DeleteCartItem(string token, int cartItemId);
        Task UpdateCartItemQuantity(string token, int cartItemId, int quantity);
        Task MergeCart(string token, int userId);
        Task AddCartItem(AddCartItemDTO req, string? token);
    }
}
