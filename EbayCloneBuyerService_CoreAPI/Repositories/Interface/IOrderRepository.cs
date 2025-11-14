using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Responses;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IOrderRepository
    {
        // Tạo Address và trả về ID
        Task<int> CreateAddressAsync(Address address);

        // Tạo OrderTable và OrderItems, trả về OrderId
        Task<int> CreateOrderAsync(OrderTable order, IEnumerable<OrderItemDto> items);

        // Optional: Xóa các mục trong giỏ hàng sau khi đặt hàng (dựa trên userId)
        Task ClearCartItemsByUserIdAsync(int userId);
    }
}
