using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(CloneEbayDbContext context) : base(context)
        {
        }

        public Coupon GetByCode(string code)
        {
            return _context.Coupons
                .Where(c => c.Code.ToUpper() == code.ToUpper())
                .FirstOrDefault();
        }

        public int GetUsageCount(int couponId)
        {
            return _context.CouponUsages
                .Count(cu => cu.CouponId == couponId);
        }

        public bool HasUserUsedCoupon(int couponId, int userId)
        {
            return _context.CouponUsages
                .Any(cu => cu.CouponId == couponId && cu.UserId == userId);
        }
    }
}
