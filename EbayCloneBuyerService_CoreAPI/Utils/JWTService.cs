using EbayCloneBuyerService_CoreAPI.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EbayCloneBuyerService_CoreAPI.Utils
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secret = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = double.Parse(jwtSettings["ExpireMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(descriptor);

            return token;
        }

       
        public bool ValidateJwtToken(string token, out ClaimsPrincipal? principal)
        {
            principal = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var secret = jwtSettings["Key"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                var handler = new JsonWebTokenHandler();
                var result = handler.ValidateToken(token, validationParameters);

                if (!result.IsValid || result.ClaimsIdentity == null)
                {
                    Console.WriteLine("Token validation FAILED: " + result.Exception?.Message);
                    return false;
                }

                principal = new ClaimsPrincipal(result.ClaimsIdentity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token validation EXCEPTION:");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }


    }
}
