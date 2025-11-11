using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IProductRepo
    {
        IEnumerable<Product> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
    }
}
