using EbayCloneBuyerService_CoreAPI.Models.Reponses;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
        }

        public async Task DeleteCartItem(string token, int cartItemId)
        {
            var checkItem = await _cartItemRepository.CheckCartItem(token, cartItemId) 
                ?? throw new Exception("Cart item not found");
            var item = _cartItemRepository.GetById(cartItemId);
            _cartItemRepository.Delete(item);
            _cartItemRepository.Save();
        }

        public async Task<IEnumerable<UserCart>> GetUserCart(string token)
        {
            return await _cartRepository.GetUserCartItemsAsync(token);
        }
        public async Task UpdateCartItemQuantity(string token, int cartItemId, int quantity)
        {
            var checkItem = await _cartItemRepository.CheckCartItem(token, cartItemId) 
                ?? throw new Exception("Cart item not found");
            var item =  _cartItemRepository.GetById(cartItemId);
            item.Quantity = quantity;
            _cartItemRepository.Update(item);
            _cartItemRepository.Save();
        }
    }
}
