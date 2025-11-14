using EbayCloneBuyerService_CoreAPI.DTOs.Profile;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IProfileService
    {
        Task<GetProfileRequest> GetProfileRequestAsync(int userId);
        Task UpdateProfileRequestAsync(UpdateProfileRequest updateProfileRequest);
    }
}
