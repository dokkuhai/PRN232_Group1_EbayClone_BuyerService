namespace EbayCloneBuyerService_CoreAPI.Models.Responses
{
    public class OrderListItemDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
        public int ItemCount { get; set; }
        public string? LatestShippingStatus { get; set; }
    }
}
