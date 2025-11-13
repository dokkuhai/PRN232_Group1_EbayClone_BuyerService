using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class ProductServices : IProductServices
    {
        private readonly IProductRepo _repo;

        public ProductServices(IProductRepo repo)
        {
            _repo = repo;
        }

        public IEnumerable<Product> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
    }
}
