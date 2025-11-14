using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Reponses;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(CloneEbayDbContext context) : base(context)
        {
        }



        public async Task<IEnumerable<UserCart>> GetUserCartItemsAsync(string token)
        {
            Cart? cart = null;
            if (int.TryParse(token, out int userId))
            {
                cart = await _context.Carts
                    .Include(c => c.Cartitems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Seller)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else
            {
                cart = await _context.Carts
     .Include(c => c.Cartitems)
         .ThenInclude(ci => ci.Product)
             .ThenInclude(p => p.Inventory)  
     .Include(c => c.Cartitems)
         .ThenInclude(ci => ci.Product)
             .ThenInclude(p => p.Seller)      
                     .FirstOrDefaultAsync(c => c.GuestToken == token);
            }

            if (cart == null)
            {
                return [];
            }

            return cart.Cartitems.Select(ci => new UserCart
            {
                CartItemId = ci.Id,
                SellerName = ci.Product.Seller?.Username ?? string.Empty,
                ProductName = ci.Product.Title ?? string.Empty,
                Quantity = ci.Quantity ?? 1,
                UnitPrice = ci.Product.Price ?? 0,
                ProductImage = ci.Product.Images ?? string.Empty,
                AvailableStock = ci.Product.Inventory.Quantity ?? 0
            });
        }
    }
}
