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
        private readonly IRememberTokenService _rememberTokenService;
        public UserController(IUserService userService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            JwtService jwtTokenGenerator,
            IRememberTokenService rememberTokenService)
        {
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _jwtTokenGenerator = jwtTokenGenerator;
            _rememberTokenService = rememberTokenService;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("No code");
            }
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

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
                return BadRequest("Can not get token.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
            Console.WriteLine("Token Response: " + json);
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var userInfoResponse = await client.GetAsync(userInfoEndpoint);
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return BadRequest("Can not get user information.");
            }

            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson);
            Console.WriteLine("User Info: " + userInfoJson);

            var user = await _userService.GetUserByEmailAsync(userInfo.Email);
            if (user == null)
            {
                Console.WriteLine("User not registered: " + userInfo.Name);
                return Unauthorized(new
                {                   
                    email = userInfo.Email,
                    userName = userInfo.Name,
                    message = "User not registered."
                });
            }
            var myJwt = _jwtTokenGenerator.GenerateToken(user);

            return Ok(new { 
                userId = user.Id,
                userName = user.Username,
                token = myJwt 
            });
        }

        [HttpPost("google-register")]
        public async Task<IActionResult> GoogleRegister([FromBody] GoogleRegisterRequest request)
        {
            var user = new Models.User
            {
                Username = request.Name,
                Email = request.Email,
            };
            await _userService.RegisterAsync(new RegisterRequest
            {
                UserName = user.Username,
                Email = user.Email,
                Password = Guid.NewGuid().ToString(), 
            });
            var registeredUser = await _userService.GetUserByEmailAsync(request.Email);
            var myJwt = _jwtTokenGenerator.GenerateToken(registeredUser);

            return Ok(new
            {
                userId = registeredUser.Id,
                userName = registeredUser.Username,
                token = myJwt
            });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.AuthenticateAsync(model);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });
            if (model.RememberMe)
            {
                var tokenhash = Guid.NewGuid().ToString();
                Response.Cookies.Append("RememberMeToken", tokenhash, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = false,            
                    Secure = false,               
                    SameSite = SameSiteMode.None,   
                    Path = "/"                    
                });
                await _rememberTokenService.AddRememberTokenAsync(user.Id, tokenhash);

            }

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

        [HttpGet("login-with-remember-token")]
        public async Task<IActionResult> LoginWithRememberToken()
        {
            if (Request.Cookies.TryGetValue("RememberMeToken", out var tokenhash))
            {
                var user = await _rememberTokenService.GetRememberTokenByHashAsync(tokenhash);
                if (user != null)
                {
                    var tokenString = _jwtTokenGenerator.GenerateToken(user);
                    return Ok(new
                    {
                        userId = user.Id,
                        userName = user.Username,
                        token = tokenString,
                    });
                }
            }
            return Unauthorized(new { message = "Invalid remember me token." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("RememberMeToken", out var tokenhash))
            {
                await _rememberTokenService.DeleteRememberTokenAsync(tokenhash);
                Response.Cookies.Delete("RememberMeToken");
            }
            return Ok(new
            {
                message = "Successfully logged out."
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid Data",
                    errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                });
            }
            await _userService.RegisterAsync(model);

                return Ok(new
                {
                    message = "Successfully registered."
                });
        }
    }
}

