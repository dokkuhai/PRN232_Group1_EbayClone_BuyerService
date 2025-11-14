using System.Globalization;

namespace EbayCloneBuyerService_CoreAPI.DTOs.User
{
    public class GoogleRegisterRequest
    {
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }
}
