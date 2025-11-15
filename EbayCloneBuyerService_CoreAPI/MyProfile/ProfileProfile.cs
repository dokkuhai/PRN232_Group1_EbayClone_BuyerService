namespace EbayCloneBuyerService_CoreAPI.MyProfile
{
    public class ProfileProfile : AutoMapper.Profile
    {
        public ProfileProfile() { 
            CreateMap<Models.User, DTOs.Profile.GetProfileRequest>();
        }
    }
}
