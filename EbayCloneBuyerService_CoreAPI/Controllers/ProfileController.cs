using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IEmailService _emailService;
        public ProfileController(IProfileService profileService, IEmailService emailService)
        {
            _profileService = profileService;
            _emailService = emailService;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfileRequestAsync(int userId)
        {
            var profile = await _profileService.GetProfileRequestAsync(userId);
            return Ok(profile);
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfileRequestAsync([FromBody] DTOs.Profile.UpdateProfileRequest updateProfileRequest)
        {
            await _profileService.UpdateProfileRequestAsync(updateProfileRequest);
            return NoContent();
        }
        [HttpPost("send-verification-email/{userId}")]
        public async Task<IActionResult> SendVerificationEmailAsync(int userId)
        {
            var user = await _profileService.GetProfileRequestAsync(userId);
            var email = user.Email;
            var subject = "Email Verification";
            var link = $"http://127.0.0.1:5500/PRN232_Group1_EbayClone_BuyerService/ui/verify-email/{userId}";
            var body = $"Please verify your email by clicking on the following link: {link}";
            await _emailService.SendAsync(email, subject, body);
            return Ok(new { Message = "Verification email sent successfully." });
        }

        [HttpPost("verify-email/{userId}")]
        public async Task<IActionResult> VerifyEmailAsync(int userId)
        {
            var user = await _profileService.GetProfileRequestAsync(userId);
            var updateProfileRequest = new DTOs.Profile.UpdateProfileRequest
            {
                UserId = userId,
                IsEmailVerified = true
            };
            await _profileService.UpdateProfileRequestAsync(updateProfileRequest);
            return Ok(new { Message = "Email verified successfully." });
        }
    }
}
