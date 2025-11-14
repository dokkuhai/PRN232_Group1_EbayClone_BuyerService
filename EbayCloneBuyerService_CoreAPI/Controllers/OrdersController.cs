using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Models.Responses;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: api/orders?buyerId=1&page=1&pageSize=10&status=Pending
        [HttpGet]
        [Authorize(Policy = "BuyerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse<List<OrderListItemDto>>>> GetOrders(
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

                var (orders, totalCount) = await _orderService.GetOrdersAsync(buyerId, page, pageSize, status);

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
        [Authorize(Policy = "BuyerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse<OrderDetailDto>>> GetOrderDetail(int orderId)
        {
            try
            {
                var orderDetail = await _orderService.GetOrderDetailAsync(orderId);

                if (orderDetail == null)
                {
                    return NotFound(new APIResponse<OrderDetailDto>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

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
        [Authorize(Policy = "BuyerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse<OrderStatusDto>>> GetOrderStatus(int orderId)
        {
            try
            {
                var statusDto = await _orderService.GetOrderStatusAsync(orderId);

                if (statusDto == null)
                {
                    return NotFound(new APIResponse<OrderStatusDto>(
                        (int)HttpStatusCode.NotFound,
                        $"Order {orderId} not found"
                    ));
                }

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
        [Authorize(Policy = "BuyerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse<ReturnRequestDto>>> CreateReturnRequest(
            int orderId,
            [FromBody] CreateReturnRequestDto request)
        {
            try
            {
                var returnDto = await _orderService.CreateReturnRequestAsync(orderId, request);

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
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service error creating return request for orderId: {OrderId}", orderId);
                var response = new APIResponse<ReturnRequestDto>(ex.StatusCode, ex.Message);
                return StatusCode(ex.StatusCode, response);
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
        [Authorize(Policy = "BuyerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse<List<ReturnRequestDto>>>> GetReturnRequests(int orderId)
        {
            try
            {
                var returnRequests = await _orderService.GetReturnRequestsAsync(orderId);

                var response = new APIResponse<List<ReturnRequestDto>>(
                    (int)HttpStatusCode.OK,
                    $"Successfully retrieved {returnRequests.Count} return requests",
                    returnRequests
                );

                return Ok(response);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service error getting return requests for orderId: {OrderId}", orderId);
                var response = new APIResponse<List<ReturnRequestDto>>(ex.StatusCode, ex.Message);
                return StatusCode(ex.StatusCode, response);
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
