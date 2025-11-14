using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IOrderService
    {
        Task<(List<OrderListItemDto> orders, int totalCount)> GetOrdersAsync(
            int buyerId,
            int page,
            int pageSize,
            string? status);

        Task<OrderDetailDto?> GetOrderDetailAsync(int orderId);

        Task<OrderStatusDto?> GetOrderStatusAsync(int orderId);

        Task<ReturnRequestDto> CreateReturnRequestAsync(
            int orderId,
            CreateReturnRequestDto request);

        Task<List<ReturnRequestDto>> GetReturnRequestsAsync(int orderId);

        Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId);
    }
}
