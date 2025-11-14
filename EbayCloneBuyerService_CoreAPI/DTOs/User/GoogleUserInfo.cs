using System.Text.Json.Serialization;

namespace EbayCloneBuyerService_CoreAPI.DTOs.User
{
    public class GoogleUserInfo
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;
        [JsonPropertyName("name")]
        public string? Name { get; set; } 
        public string? Picture { get; set; }
    }
}
