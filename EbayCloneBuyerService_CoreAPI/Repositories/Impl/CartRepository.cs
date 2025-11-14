using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Responses;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(CloneEbayDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetCartByGuestToken(string token)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.GuestToken == token);
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetCartByTokenOrId(string token)
        {
            if (int.TryParse(token, out int userId))
            {
                return await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else
            {
                return await _context.Carts.FirstOrDefaultAsync(c => c.GuestToken == token);
            }
        }

        public async Task<IEnumerable<UserCart>> GetUserCartItemsAsync(string token)
        {
            Cart? cart = null;
            if (int.TryParse(token, out int userId))
            {
                cart = await _context.Carts
    .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
            .ThenInclude(p => p.Inventories)
    .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
            .ThenInclude(p => p.Seller)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else
            {
                cart = await _context.Carts
     .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Product)
             .ThenInclude(p => p.Inventories)
     .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Product)
             .ThenInclude(p => p.Seller)
                     .FirstOrDefaultAsync(c => c.GuestToken == token);
            }

            if (cart == null)
            {
                return [];
            }

            return cart.CartItems.Select(ci => new UserCart
            {
                CartItemId = ci.Id,
                SellerName = ci.Product.Seller?.Username ?? string.Empty,
                ProductName = ci.Product.Title ?? string.Empty,
                Quantity = ci.Quantity ?? 1,
                UnitPrice = ci.Product.Price ?? 0,
                ProductImage = ci.Product.Images ?? string.Empty,
                AvailableStock = ci.Product.Inventories?.Sum(i => i.Quantity) ?? 0
            });
        }
    }
}
