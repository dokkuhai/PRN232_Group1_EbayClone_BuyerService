using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.DTOs.User
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}
