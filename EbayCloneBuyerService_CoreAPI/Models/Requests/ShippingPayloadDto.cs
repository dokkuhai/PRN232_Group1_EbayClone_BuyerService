namespace EbayCloneBuyerService_CoreAPI.Models.Requests
{
    public class ShippingPayloadDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
