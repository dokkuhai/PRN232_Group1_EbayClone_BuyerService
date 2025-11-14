namespace EbayCloneBuyerService_CoreAPI.DTOs.Profile
{
    public class GetProfileRequest
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
