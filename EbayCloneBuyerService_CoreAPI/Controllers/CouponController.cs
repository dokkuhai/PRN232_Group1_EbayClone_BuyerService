using EbayCloneBuyerService_CoreAPI.DTOs;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/coupon")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost("apply")]
        public IActionResult Apply([FromQuery] string code, [FromQuery] int productId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Không tìm thấy userId trong token.");

            int userId = int.Parse(userIdClaim);

            var result = _couponService.ApplyCoupon(code, productId, userId);
            return Ok(result);
        }
    }

}
