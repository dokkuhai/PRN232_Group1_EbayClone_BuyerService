using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICouponRepository
    {
        Coupon GetByCode(string code);
        int GetUsageCount(int couponId);
        void AddUsage(CouponUsage usage);
    }
}
