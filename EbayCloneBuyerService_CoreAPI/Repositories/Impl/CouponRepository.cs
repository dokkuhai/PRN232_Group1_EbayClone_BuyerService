using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using System;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class CouponRepository : ICouponRepository
    {
        private readonly CloneEbayDbContext _context;

        public CouponRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public Coupon GetByCode(string code)
        {
            return _context.Coupons
                           .FirstOrDefault(c => c.Code == code);
        }

        public int GetUsageCount(int couponId)
        {
            return _context.CouponUsages
                           .Count(u => u.CouponId == couponId);
        }

        public void AddUsage(CouponUsage usage)
        {
            _context.CouponUsages.Add(usage);
            _context.SaveChanges();
        }
    }

}
