using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CartItemRepository : GenericRepository<Cartitem>, ICartItemRepository
    {
        public CartItemRepository(CloneEbayDbContext context) : base(context)
        {
        }
        public async Task<Cartitem?> CheckCartItem(string token, int cartItemId)
        {
            Cartitem? cartItem = null;
            if (int.TryParse(token, out int userId))
            {
                cartItem = await _context.Cartitems.Include(ci => ci.Cart)
                   .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
            }
            else
            {
                cartItem = await _context.Cartitems.Include(ci => ci.Cart)
                   .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.GuestToken == token);
            }
            return cartItem;
        }
    }
}
