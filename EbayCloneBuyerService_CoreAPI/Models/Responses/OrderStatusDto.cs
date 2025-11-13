namespace EbayCloneBuyerService_CoreAPI.Models.Responses
{
    public class OrderStatusDto
    {
        public int OrderId { get; set; }
        public string? OrderStatus { get; set; }
        public string? ShippingStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedArrival { get; set; }
    }
}
