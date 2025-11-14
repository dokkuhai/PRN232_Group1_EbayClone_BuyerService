using AutoMapper;
using EbayCloneBuyerService_CoreAPI.DTOs.User;
using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.MyProfile
{
    public class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<RegisterRequest, User>();
            CreateMap<LoginRequest, User>();
        }
    }
}
