using System.Text.Json.Serialization;

namespace EbayCloneBuyerService_CoreAPI.DTOs.User
{
    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }
}
