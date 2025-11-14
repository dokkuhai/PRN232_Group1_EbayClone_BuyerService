using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<CartItem> CheckCartItem(string token, int cartItemId);
        Task<CartItem?> CheckCartItemAsync(int cartId, int productId);

    }
}
