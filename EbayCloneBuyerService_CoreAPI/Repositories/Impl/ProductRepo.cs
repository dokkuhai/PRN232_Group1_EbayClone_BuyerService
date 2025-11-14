using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class ProductRepo : IProductRepo
    {
        private readonly CloneEbayDbContext _context;

        public ProductRepo(CloneEbayDbContext context)
        {
            _context = context;
        }

        public  IQueryable<Product> GetAllAsync()
        {
            return _context.Products.Include(p => p.Category).Include(s => s.Seller).
                Include(i => i.Inventories).Include(r => r.Reviews).Include(c => c.Coupons)
                .AsQueryable();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).Include(s => s.Seller).
                Include(i => i.Inventories).Include(r => r.Reviews).Include(c => c.Coupons)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
