using System.Security.Claims;

namespace EbayCloneBuyerService_CoreAPI.Utils
{
    public static class Utilities
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var userId) ? userId : null;
        }
        public static string GenerateGuestToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
