using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(CloneEbayDbContext context) : base(context)
        {
        }
        public async Task<CartItem?> CheckCartItem(string token, int cartItemId)
        {
            CartItem? cartItem = null;
            if (int.TryParse(token, out int userId))
            {
                cartItem = await _context.CartItems.Include(ci => ci.Cart)
                   .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
            }
            else
            {
                cartItem = await _context.CartItems.Include(ci => ci.Cart)
                   .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.GuestToken == token);
            }
            return cartItem;
        }
    }
}
