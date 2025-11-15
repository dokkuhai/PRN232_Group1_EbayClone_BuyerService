using EbayCloneBuyerService_CoreAPI.DTOs;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepo;
        private readonly IProductRepo _productRepo;

        public CouponService(ICouponRepository couponRepo, IProductRepo productRepo)
        {
            _couponRepo = couponRepo;
            _productRepo = productRepo;
        }

        public CouponApplyResult ApplyCoupon(string code, int productId, int userId)
        {
            var coupon = _couponRepo.GetByCode(code);
            if (coupon == null)
            {
                return new CouponApplyResult { Valid = false, Message = "Coupon không hợp lệ" };
            }

            // Kiểm tra ngày
            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                return new CouponApplyResult { Valid = false, Message = "Coupon đã hết hạn" };
            }

            // Kiểm tra số lượt
            int usageCount = _couponRepo.GetUsageCount(coupon.Id);
            if (usageCount >= coupon.MaxUsage)
            {
                return new CouponApplyResult { Valid = false, Message = "Coupon đã hết lượt sử dụng" };
            }

            // Kiểm tra sản phẩm
            if (coupon.ProductId != null && coupon.ProductId != productId)
            {
                return new CouponApplyResult { Valid = false, Message = "Coupon không áp dụng cho sản phẩm này" };
            }

            // Lấy giá sản phẩm
            var product = _productRepo.GetByIdAsync(productId).Result;
            if (product == null)
            {
                return new CouponApplyResult { Valid = false, Message = "Không tìm thấy sản phẩm" };
            }

            // Tính giảm giá
            var discountAmount = product.Price * (coupon.DiscountPercent / 100);
            var finalPrice = product.Price - discountAmount;

            return new CouponApplyResult
            {
                Valid = true,
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice
            };
        }
    }

}
