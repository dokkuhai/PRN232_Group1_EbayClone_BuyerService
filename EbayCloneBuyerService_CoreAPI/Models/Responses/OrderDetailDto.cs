namespace EbayCloneBuyerService_CoreAPI.Models.Responses
{
    public class OrderDetailDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<ShippingInfoDto> ShippingInfos { get; set; } = new();
        public List<ReturnRequestDto> ReturnRequests { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Subtotal { get; set; }
    }

    public class ShippingInfoDto
    {
        public int Id { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public DateTime? EstimatedArrival { get; set; }
    }

    public class ReturnRequestDto
    {
        public int Id { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}
