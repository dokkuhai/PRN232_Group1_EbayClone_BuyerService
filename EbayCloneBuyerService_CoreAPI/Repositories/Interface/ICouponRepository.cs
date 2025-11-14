using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICouponRepository : IGenericRepository<Coupon>
    {
        Coupon GetByCode(string code);
        int GetUsageCount(int couponId);
        bool HasUserUsedCoupon(int couponId, int userId);
    }
}
