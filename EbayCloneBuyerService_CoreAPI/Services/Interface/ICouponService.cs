using EbayCloneBuyerService_CoreAPI.DTOs;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface ICouponService
    {
        CouponApplyResult ApplyCoupon(string code, int productId, int userId);
    }
}
