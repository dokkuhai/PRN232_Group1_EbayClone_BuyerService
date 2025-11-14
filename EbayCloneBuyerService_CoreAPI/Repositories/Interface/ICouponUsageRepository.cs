using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface ICouponUsageRepository : IGenericRepository<CouponUsage>
    {
        int CountByCouponId(int couponId);
        bool ExistsByUserAndCoupon(int userId, int couponId);
    }
}
