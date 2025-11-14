using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly CloneEbayDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(CloneEbayDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<OrderListItemDto> orders, int totalCount)> GetOrdersAsync(
            int buyerId,
            int page,
            int pageSize,
            string? status)
        {
            _logger.LogInformation("Getting orders for buyerId: {BuyerId}, page: {Page}, pageSize: {PageSize}, status: {Status}",
                buyerId, page, pageSize, status);

            var query = _context.OrderTables
                .Where(o => o.BuyerId == buyerId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListItemDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    ItemCount = o.OrderItems.Count,
                    LatestShippingStatus = o.ShippingInfos
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.Status)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId)
        {
            _logger.LogInformation("Getting order detail for orderId: {OrderId}", orderId);

            var order = await _context.OrderTables
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingInfos)
                .Include(o => o.ReturnRequests)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            return new OrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                ShippingAddress = order.Address != null ? new AddressDto
                {
                    Street = order.Address.Street,
                    City = order.Address.City,
                    State = order.Address.State,
                    PostalCode = null,
                    Country = order.Address.Country
                } : null,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId ?? 0,
                    ProductName = oi.Product?.Title,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = (oi.Quantity ?? 0) * (oi.UnitPrice ?? 0)
                }).ToList(),
                ShippingInfos = order.ShippingInfos.Select(si => new ShippingInfoDto
                {
                    Id = si.Id,
                    Carrier = si.Carrier,
                    TrackingNumber = si.TrackingNumber,
                    Status = si.Status,
                    EstimatedArrival = si.EstimatedArrival
                }).OrderByDescending(s => s.Id).ToList(),
                ReturnRequests = order.ReturnRequests.Select(rr => new ReturnRequestDto
                {
                    Id = rr.Id,
                    Reason = rr.Reason,
                    Status = rr.Status,
                    CreatedAt = rr.CreatedAt
                }).OrderByDescending(r => r.CreatedAt).ToList()
            };
        }

        public async Task<OrderStatusDto?> GetOrderStatusAsync(int orderId)
        {
            _logger.LogInformation("Getting order status for orderId: {OrderId}", orderId);

            var order = await _context.OrderTables
                .Include(o => o.ShippingInfos)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            var latestShipping = order.ShippingInfos
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

            return new OrderStatusDto
            {
                OrderId = order.Id,
                OrderStatus = order.Status,
                ShippingStatus = latestShipping?.Status,
                TrackingNumber = latestShipping?.TrackingNumber,
                EstimatedArrival = latestShipping?.EstimatedArrival
            };
        }

        public async Task<ReturnRequestDto> CreateReturnRequestAsync(
            int orderId,
            CreateReturnRequestDto request)
        {
            _logger.LogInformation("Creating return request for orderId: {OrderId}, userId: {UserId}",
                orderId, request.UserId);

            var order = await _context.OrderTables
                .Include(o => o.ReturnRequests)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new ServiceException($"Order {orderId} not found", 404);
            }

            if (order.BuyerId != request.UserId)
            {
                throw new ServiceException("Order does not belong to this user", 400);
            }

            var hasPendingReturn = order.ReturnRequests
                .Any(rr => rr.Status == "Pending");

            if (hasPendingReturn)
            {
                throw new ServiceException("There is already a pending return request for this order", 400);
            }

            var returnRequest = new ReturnRequest
            {
                OrderId = orderId,
                UserId = request.UserId,
                Reason = request.Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            return new ReturnRequestDto
            {
                Id = returnRequest.Id,
                Reason = returnRequest.Reason,
                Status = returnRequest.Status,
                CreatedAt = returnRequest.CreatedAt
            };
        }

        public async Task<List<ReturnRequestDto>> GetReturnRequestsAsync(int orderId)
        {
            _logger.LogInformation("Getting return requests for orderId: {OrderId}", orderId);

            var order = await _context.OrderTables
                .Include(o => o.ReturnRequests)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new ServiceException($"Order {orderId} not found", 404);
            }

            return order.ReturnRequests
                .Select(rr => new ReturnRequestDto
                {
                    Id = rr.Id,
                    Reason = rr.Reason,
                    Status = rr.Status,
                    CreatedAt = rr.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public async Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId)
        {
            var order = await _context.OrderTables
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order != null && order.BuyerId == userId;
        }
    }
}
