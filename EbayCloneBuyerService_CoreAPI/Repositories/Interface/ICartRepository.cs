using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Reponses;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<IEnumerable<UserCart>> GetUserCartItemsAsync(string token);
    }
}
