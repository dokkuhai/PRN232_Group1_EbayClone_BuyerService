using System.Net.Http.Headers;
using System.Text.Json;

namespace EbayCloneBuyerService_CoreAPI.Utils
{
    public class GoogleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GoogleService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task<string?> GetAccessTokenAsync(string code)
        {
            var values = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _config["GoogleAuth:ClientId"] },
            { "client_secret", _config["GoogleAuth:ClientSecret"] },
            { "redirect_uri", _config["GoogleAuth:RedirectUri"] },
            { "grant_type", "authorization_code" }
        };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<GoogleUser?> GetUserInfoAsync(string code)
        {
            var accessToken = await GetAccessTokenAsync(code);
            if (accessToken == null)
                return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleUser>(json);
        }
    }
    public class GoogleUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }
}
