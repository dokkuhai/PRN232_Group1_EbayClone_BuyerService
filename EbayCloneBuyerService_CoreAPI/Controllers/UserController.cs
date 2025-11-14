using EbayCloneBuyerService_CoreAPI.DTOs.User;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using EbayCloneBuyerService_CoreAPI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtTokenGenerator;
        public UserController(IUserService userService, IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtTokenGenerator)
        {
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Không có code");
            }

            // === 1. Đổi 'code' lấy 'access_token' và 'id_token' ===
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var tokenParams = new Dictionary<string, string>
        {
    { "client_id", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") },
    { "client_secret", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") },
    { "code", request.Code },
    { "grant_type", "authorization_code" },
    { "redirect_uri", Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI") }
};

            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenParams));
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Không thể đổi code lấy token.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
            Console.WriteLine("Token Response: " + json);
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var userInfoResponse = await client.GetAsync(userInfoEndpoint);
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return BadRequest("Không thể lấy thông tin người dùng.");
            }

            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson);

            var myJwt = "day_la_token_jwt_cua_ban_sau_khi_tao_xong";

            // === 5. Trả JWT về cho FE ===
            return Ok(new { token = myJwt });
        }
    

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.AuthenticateAsync(model);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var roleName = user.Role??"User";
            if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                roleName = "Admin";

            var tokenString = _jwtTokenGenerator.GenerateToken(user);

            return Ok(new
            {
                token = tokenString,
                userId = user.Id,
                userName = user.Username
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                });
            }
            await _userService.RegisterAsync(model);

                return Ok(new
                {
                    message = "Đăng ký tài khoản thành công! Vui lòng đăng nhập."
                });
        }
    }
}

