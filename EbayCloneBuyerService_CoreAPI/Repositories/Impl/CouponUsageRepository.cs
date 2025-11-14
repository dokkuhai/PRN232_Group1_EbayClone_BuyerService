using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CouponUsageRepository : GenericRepository<CouponUsage>, ICouponUsageRepository
    {
        public CouponUsageRepository(CloneEbayDbContext context) : base(context)
        {
        }

        public int CountByCouponId(int couponId)
        {
            return _context.CouponUsages
                .Count(cu => cu.CouponId == couponId);
        }

        public bool ExistsByUserAndCoupon(int userId, int couponId)
        {
            return _context.CouponUsages
                .Any(cu => cu.UserId == userId && cu.CouponId == couponId);
        }
    }
}
