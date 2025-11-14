using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Models.Reponses;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using EbayCloneBuyerService_CoreAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly JwtService _jwtHelper;
        public CartController(ICartService cartService, JwtService jwtHelper)
        {
            _cartService = cartService;
            _jwtHelper = jwtHelper;
        }
        /// <summary>
        /// Get user cart items by JWT token or guest token
        /// </summary>
        /// <param name="token">pass guest token if it is in local storage else ignore this field</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserCart([FromQuery] string? token)
        {
            var jwtToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(jwtToken) && _jwtHelper.ValidateJwtToken(jwtToken, out var principal))
            {
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Invalid token",
                        Data = null
                    });
                }

                var cartItems = await _cartService.GetUserCart(userId);
                return Ok(new APIResponse<IEnumerable<UserCart>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User cart retrieved successfully",
                    Data = cartItems
                });
            }

            if (token != null)
            {
                return Ok(new APIResponse<IEnumerable<UserCart>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User cart retrieved successfully",
                    Data = await _cartService.GetUserCart(token)
                });
            }
            return Unauthorized(new APIResponse<object>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "User not authenticated",
                Data = null
            });
        }

        /// <summary>
        /// Deletes a cart item identified by the specified ID for the user associated with the provided authentication
        /// token.
        /// </summary>
        /// <param name="token">The authentication token representing the user whose cart item will be deleted. Cannot be null or empty.</param>
        /// <param name="id">The unique identifier of the cart item to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation. Returns a 200 OK response if
        /// the item is deleted successfully; otherwise, returns a 404 Not Found response if the item does not exist or
        /// cannot be deleted.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem([FromQuery] string? token, [FromRoute] int id)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(jwtToken) && _jwtHelper.ValidateJwtToken(jwtToken, out var principal))
                {
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized(new APIResponse<object>
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                    await _cartService.DeleteCartItem(userId, id);
                }
                if (token != null)
                {
                    await _cartService.DeleteCartItem(token, id);
                    return Ok(new APIResponse<object>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Cart item deleted successfully",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return NotFound(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            return NotFound(new APIResponse<object>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "No cart found",
                Data = null
            });
        }
        /// <summary>
        /// Updates the quantity of a specific item in the user's shopping cart.
        /// </summary>
        /// <param name="token">The authentication token that identifies the user whose cart will be updated. Cannot be null or empty.</param>
        /// <param name="id">The unique identifier of the cart item to update.</param>
        /// <param name="req">An object containing the new quantity for the cart item. The quantity must be a positive integer.</param>
        /// <returns>An IActionResult containing an API response that indicates whether the cart item quantity was updated
        /// successfully. Returns a 200 OK response if successful; otherwise, returns a 404 Not Found response if the
        /// item does not exist.</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCartItemQuantity(
            [FromQuery] string? token,
            [FromRoute] int id,
            [FromBody] UpdateCartDTO req)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(jwtToken) && _jwtHelper.ValidateJwtToken(jwtToken, out var principal))
                {
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized(new APIResponse<object>
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                    await _cartService.UpdateCartItemQuantity(userId, id, req.Quantity);
                }
                if (token != null)
                {
                    await _cartService.UpdateCartItemQuantity(token, id, req.Quantity);
                    return Ok(new APIResponse<object>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Cart item quantity updated successfully",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return NotFound(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            return NotFound(new APIResponse<object>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "No cart found",
                Data = null
            });
        }

        /// <summary>
        /// Merges the items from a guest user's cart, identified by the specified token, into the authenticated user's
        /// cart.
        /// </summary>
        /// <remarks>This operation requires the user to be authenticated. After merging, the guest cart
        /// is combined with the user's existing cart, and the guest cart is no longer accessible.</remarks>
        /// <param name="guestToken">The token that uniquely identifies the guest user's cart to be merged. Cannot be null or empty.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the merge operation. Returns a 200 OK response if
        /// the cart is merged successfully.</returns>
        /// <exception cref="Exception">Thrown if the authenticated user cannot be determined from the current context.</exception>
        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> MergeCart([FromQuery] string guestToken)
        {
            int userId = User.GetUserId() ?? throw new Exception("User not found");
            await _cartService.MergeCart(guestToken, userId);
            return Ok(new APIResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Cart merged successfully",
                Data = null
            });
        }
        /// <summary>
        /// Adds an item to the user's shopping cart using the provided item details and authentication token.
        /// </summary>
        /// <remarks>If a valid JWT token is present in the Authorization header, it is used to identify
        /// the user. Otherwise, the method falls back to the token provided in the query string. The method does not
        /// return the added cart item; it only indicates the success or failure of the operation.</remarks>
        /// <param name="req">An object containing the details of the cart item to add, such as product identifier, quantity, and other
        /// relevant information.</param>
        /// <param name="token">An optional authentication token used to identify the user if a valid JWT is not present in the request
        /// headers.</param>
        /// <returns>An IActionResult indicating the result of the operation. Returns a 200 OK response if the item is added
        /// successfully, or a 401 Unauthorized response if authentication fails.</returns>
        [HttpPost]
        public async Task<IActionResult> AddCartItem(AddCartItemDTO req, [FromQuery] string? token)
        {
            var jwtToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(jwtToken) && _jwtHelper.ValidateJwtToken(jwtToken, out var principal))
            {
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Invalid token",
                        Data = null
                    });
                }
                await _cartService.AddCartItem(req, userId);
                return Ok(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Cart item added successfully",
                    Data = null
                });
            }
            else
            {
                await _cartService.AddCartItem(req, token);
                return Ok(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Cart item added successfully",
                    Data = null
                });
            }
        }
    }
}
