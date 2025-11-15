namespace EbayCloneBuyerService_CoreAPI.Models.Requests
{
    public class OrderItemRequestDto
    {
        public int ProductId { get; set; } // Phải là ProductId thật, không phải CartItemId
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
