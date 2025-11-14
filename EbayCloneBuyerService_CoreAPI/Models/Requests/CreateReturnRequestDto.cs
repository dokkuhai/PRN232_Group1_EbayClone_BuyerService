namespace EbayCloneBuyerService_CoreAPI.Models.Requests
{
    public class CreateReturnRequestDto
    {
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
