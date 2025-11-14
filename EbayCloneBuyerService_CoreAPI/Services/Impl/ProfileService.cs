using AutoMapper;
using EbayCloneBuyerService_CoreAPI.DTOs.Profile;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository profileRepository;
        private readonly IMapper mapper;
        public ProfileService(IProfileRepository profileRepository, IMapper mapper)
        {
            this.profileRepository = profileRepository;
            this.mapper = mapper;
        }
        public async Task<GetProfileRequest> GetProfileRequestAsync(int userId)
        {
            var profile = await profileRepository.GetUserByIdAsync(userId);
            var profileDto = mapper.Map<GetProfileRequest>(profile);
            return profileDto;
        }

        public async Task UpdateProfileRequestAsync(UpdateProfileRequest updateProfileRequest)
        {
            if (updateProfileRequest.UserName != null)
            {
                await profileRepository.UpdateUserNameAsync(updateProfileRequest.UserId, updateProfileRequest.UserName);
            }
            else if (updateProfileRequest.Email != null)
            {
                await profileRepository.UpdateUserEmailAsync(updateProfileRequest.UserId, updateProfileRequest.Email);
            }
            else if (updateProfileRequest.PhoneNumber != null)
            {
                await profileRepository.UpdateUserPhoneNumberAsync(updateProfileRequest.UserId, updateProfileRequest.PhoneNumber);
            }
            else if (updateProfileRequest.IsEmailVerified == true)
            {
                await profileRepository.UpdateVerifiedEmailStatusAsync(updateProfileRequest.UserId, true);
            }
        }
    }
}
