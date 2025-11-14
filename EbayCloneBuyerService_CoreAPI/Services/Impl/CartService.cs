using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.OData.UriParser;

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

        public async Task AddCartItem(AddCartItemDTO req, string? token)
        {
            if (int.TryParse(token, out int userId))
            {
                var userCart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (userCart == null)
                {
                    userCart = new Cart
                    {
                        UserId = userId
                    };
                    _cartRepository.Add(userCart);
                    _cartRepository.Save();
                }
                var existingItem = await _cartItemRepository.CheckCartItemAsync(userCart.Id, req.ProductId);
                if (existingItem == null)
                {
                    existingItem.Quantity += req.Quantity;
                    _cartItemRepository.Update(existingItem);
                    _cartItemRepository.Save();
                    return;
                }
                _cartItemRepository.Add(new CartItem
                {
                    CartId = userCart.Id,
                    ProductId = req.ProductId,
                    Quantity = req.Quantity
                });
                _cartItemRepository.Save();
            }
            else
            {
                var guestCart = await _cartRepository.GetCartByGuestToken(token);
                if (guestCart == null)
                {
                    guestCart = new Cart
                    {
                        GuestToken = token
                    };
                    _cartRepository.Add(guestCart);
                    _cartRepository.Save();
                }
                var existingItem = await _cartItemRepository.CheckCartItemAsync(guestCart.Id, req.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += req.Quantity;
                    _cartItemRepository.Update(existingItem);
                    _cartItemRepository.Save();
                    return;
                }
                _cartItemRepository.Add(new CartItem
                {
                    CartId = guestCart.Id,
                    ProductId = req.ProductId,
                    Quantity = req.Quantity
                });
                _cartItemRepository.Save();
            }
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

        public async Task MergeCart(string token, int userId)
        {
            var userCart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (userCart != null)
            {
                return;
            }
            var currentCart = await _cartRepository.GetCartByGuestToken(token);
            if (currentCart == null)
            {
                return;
            }
            currentCart.UserId = userId;
            _cartRepository.Update(currentCart);
            _cartRepository.Save();
        }

        public async Task UpdateCartItemQuantity(string token, int cartItemId, int quantity)
        {
            var checkItem = await _cartItemRepository.CheckCartItem(token, cartItemId)
                ?? throw new Exception("Cart item not found");
            var item = _cartItemRepository.GetById(cartItemId);
            item.Quantity = quantity;
            _cartItemRepository.Update(item);
            _cartItemRepository.Save();
        }
    }
}
