namespace EbayCloneBuyerService_CoreAPI.DTOs
{
    public class CouponApplyResult
    {
        public bool Valid { get; set; }
        public string Message { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
