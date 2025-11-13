using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IProductServices
    {
        IEnumerable<Product> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
    }
}
