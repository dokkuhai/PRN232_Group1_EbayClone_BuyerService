using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Reponses;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly CloneEbayDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(CloneEbayDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/orders?buyerId=1&page=1&pageSize=10&status=Pending
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<APIResponse<List<OrderListItemDto>>> GetOrders(
            [FromQuery] int buyerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                // Validate user from JWT token
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != buyerId.ToString())
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new APIResponse<List<OrderListItemDto>>(
                        (int)HttpStatusCode.Forbidden,
                        "You are not authorized to view orders for this buyer",
                        null));
                }

                if (buyerId <= 0)
                {
                    return BadRequest(new APIResponse<List<OrderListItemDto>>(
                        (int)HttpStatusCode.BadRequest,
                        "BuyerId is required and must be greater than 0"
                    ));
                }

                _logger.LogInformation("Getting orders for buyerId: {BuyerId}, page: {Page}, pageSize: {PageSize}, status: {Status}",
                    buyerId, page, pageSize, status);

                var query = _context.OrderTables
                    .Where(o => o.BuyerId == buyerId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                var totalCount = query.Count();

                var orders = query
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
                    .ToList();

                var response = new APIResponse<List<OrderListItemDto>>(
                    (int)HttpStatusCode.OK,
                    $"Successfully retrieved {orders.Count} orders. Total: {totalCount}",
                    orders
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for buyerId: {BuyerId}", buyerId);
                var response = new APIResponse<List<OrderListItemDto>>(
                    (int)HttpStatusCode.InternalServerError,
                    ex.Message
                );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // GET: api/orders/5
        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse<OrderDetailDto>> GetOrderDetail(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting order detail for orderId: {OrderId}", orderId);

                var order = _context.OrderTables
                    .Include(o => o.Address)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.ShippingInfos)
                    .Include(o => o.ReturnRequests)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new APIResponse<OrderDetailDto>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

                var orderDetail = new OrderDetailDto
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
                        PostalCode = null, // Not in database schema
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

                var response = new APIResponse<OrderDetailDto>(
                    (int)HttpStatusCode.OK,
                    "Successfully retrieved order detail",
                    orderDetail
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order detail for orderId: {OrderId}", orderId);
                var response = new APIResponse<OrderDetailDto>(
                    (int)HttpStatusCode.InternalServerError,
                    ex.Message
                );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // GET: api/orders/5/status
        [HttpGet("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse<OrderStatusDto>> GetOrderStatus(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting order status for orderId: {OrderId}", orderId);

                var order = _context.OrderTables
                    .Include(o => o.ShippingInfos)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new APIResponse<OrderStatusDto>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

                var latestShipping = order.ShippingInfos
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefault();

                var statusDto = new OrderStatusDto
                {
                    OrderId = order.Id,
                    OrderStatus = order.Status,
                    ShippingStatus = latestShipping?.Status,
                    TrackingNumber = latestShipping?.TrackingNumber,
                    EstimatedArrival = latestShipping?.EstimatedArrival
                };

                var response = new APIResponse<OrderStatusDto>(
                    (int)HttpStatusCode.OK,
                    "Successfully retrieved order status",
                    statusDto
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status for orderId: {OrderId}", orderId);
                var response = new APIResponse<OrderStatusDto>(
                    (int)HttpStatusCode.InternalServerError,
                    ex.Message
                );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // POST: api/orders/5/return-requests
        [HttpPost("{orderId}/return-requests")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse<ReturnRequestDto>> CreateReturnRequest(
            int orderId,
            [FromBody] CreateReturnRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating return request for orderId: {OrderId}, userId: {UserId}",
                    orderId, request.UserId);

                // Validate order exists and belongs to user
                var order = _context.OrderTables
                    .Include(o => o.ReturnRequests)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new APIResponse<ReturnRequestDto>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

                if (order.BuyerId != request.UserId)
                {
                    return BadRequest(new APIResponse<ReturnRequestDto>(
                        (int)HttpStatusCode.BadRequest,
                        "Order does not belong to this user"
                    ));
                }

                // Check if there's already a pending return request
                var hasPendingReturn = order.ReturnRequests
                    .Any(rr => rr.Status == "Pending");

                if (hasPendingReturn)
                {
                    return BadRequest(new APIResponse<ReturnRequestDto>(
                        (int)HttpStatusCode.BadRequest,
                        "There is already a pending return request for this order"
                    ));
                }

                // Create return request
                var returnRequest = new ReturnRequest
                {
                    OrderId = orderId,
                    UserId = request.UserId,
                    Reason = request.Reason,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.ReturnRequests.Add(returnRequest);
                _context.SaveChanges();

                var returnDto = new ReturnRequestDto
                {
                    Id = returnRequest.Id,
                    Reason = returnRequest.Reason,
                    Status = returnRequest.Status,
                    CreatedAt = returnRequest.CreatedAt
                };

                var response = new APIResponse<ReturnRequestDto>(
                    (int)HttpStatusCode.Created,
                    "Return request created successfully",
                    returnDto
                );

                return CreatedAtAction(
                    nameof(GetOrderDetail),
                    new { orderId = orderId },
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating return request for orderId: {OrderId}", orderId);
                var response = new APIResponse<ReturnRequestDto>(
                    (int)HttpStatusCode.InternalServerError,
                    ex.Message
                );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // GET: api/orders/5/return-requests
        [HttpGet("{orderId}/return-requests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse<List<ReturnRequestDto>>> GetReturnRequests(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting return requests for orderId: {OrderId}", orderId);

                var order = _context.OrderTables
                    .Include(o => o.ReturnRequests)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new APIResponse<List<ReturnRequestDto>>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

                var returnRequests = order.ReturnRequests
                    .Select(rr => new ReturnRequestDto
                    {
                        Id = rr.Id,
                        Reason = rr.Reason,
                        Status = rr.Status,
                        CreatedAt = rr.CreatedAt
                    })
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                var response = new APIResponse<List<ReturnRequestDto>>(
                    (int)HttpStatusCode.OK,
                    $"Successfully retrieved {returnRequests.Count} return requests",
                    returnRequests
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting return requests for orderId: {OrderId}", orderId);
                var response = new APIResponse<List<ReturnRequestDto>>(
                    (int)HttpStatusCode.InternalServerError,
                    ex.Message
                );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
