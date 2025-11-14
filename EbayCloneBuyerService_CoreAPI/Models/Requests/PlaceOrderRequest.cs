using EbayCloneBuyerService_CoreAPI.Models.Responses;

namespace EbayCloneBuyerService_CoreAPI.Models.Requests
{
    public class PlaceOrderRequest
    {
        public ShippingPayloadDto ShippingInfo { get; set; }
        public List<OrderItemRequestDto> Items { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxRate { get; set; } = 0.08m;
    }
}
