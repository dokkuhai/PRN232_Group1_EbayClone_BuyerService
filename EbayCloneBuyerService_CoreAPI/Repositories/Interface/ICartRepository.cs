using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Responses;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<IEnumerable<UserCart>> GetUserCartItemsAsync(string token);
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Cart?> GetCartByGuestToken(string token);
        Task<Cart?> GetCartByTokenOrId(string token);

    }
}
