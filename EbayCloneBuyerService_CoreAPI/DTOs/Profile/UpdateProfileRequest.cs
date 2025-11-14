namespace EbayCloneBuyerService_CoreAPI.DTOs.Profile
{
    public class UpdateProfileRequest
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsEmailVerified { get; set; }
    }
}
