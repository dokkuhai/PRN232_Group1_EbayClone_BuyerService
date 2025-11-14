namespace EbayCloneBuyerService_CoreAPI.Models.Reponses
{
    public class UserCart
    {
        public int CartItemId { get; set; }
        public required string SellerName { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public required string ProductImage { get; set; }
        public int AvailableStock { get; set; }

    }
}
