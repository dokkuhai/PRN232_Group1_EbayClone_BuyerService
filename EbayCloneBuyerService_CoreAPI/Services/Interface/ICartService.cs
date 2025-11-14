using EbayCloneBuyerService_CoreAPI.Models.Reponses;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface ICartService
    {
        Task<IEnumerable<UserCart>> GetUserCart(string token);
        Task DeleteCartItem(string token, int cartItemId);
        Task UpdateCartItemQuantity(string token, int cartItemId, int quantity);
    }
}
