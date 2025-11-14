using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICartItemRepository : IGenericRepository<Cartitem>
    {
        Task<Cartitem> CheckCartItem(string token, int cartItemId);

    }
}
