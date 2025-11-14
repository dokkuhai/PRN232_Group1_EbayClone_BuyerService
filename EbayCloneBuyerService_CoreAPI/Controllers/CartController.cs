using EbayCloneBuyerService_CoreAPI.Models.Reponses;
using EbayCloneBuyerService_CoreAPI.Models.Requests;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserCart([FromQuery] string token)
        {
            return Ok(new APIResponse<IEnumerable<UserCart>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User cart retrieved successfully",
                Data = await _cartService.GetUserCart(token)
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem([FromQuery] string token, [FromRoute] int id)
        {
            try
            {
                await _cartService.DeleteCartItem(token, id);
                return Ok(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Cart item deleted successfully",
                    Data = null
                });
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
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCartItemQuantity(
            [FromQuery] string token,
            [FromRoute] int id,
            [FromBody] UpdateCartDTO req)
        {
            try
            {
                await _cartService.UpdateCartItemQuantity(token, id, req.Quantity);
                return Ok(new APIResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Cart item quantity updated successfully",
                    Data = null
                });
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
        }
    }
}
