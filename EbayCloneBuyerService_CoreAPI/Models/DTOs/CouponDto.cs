using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.Models.DTOs
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
        public int? ProductId { get; set; }
        public string ProductTitle { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => DateTime.Now > EndDate;
        public bool IsFullyUsed => CurrentUsage >= MaxUsage;
    }

    public class ValidateCouponDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int? ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal OrderTotal { get; set; }
    }

    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public CouponDto Coupon { get; set; }
    }

    public class ApplyCouponDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int OrderId { get; set; }
    }
}
