using AutoMapper;
using EbayCloneBuyerService_CoreAPI.DTOs.User;
using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<User?> AuthenticateAsync(LoginRequest loginRequest)
        {
            var email = loginRequest.Email;
            return await _userRepository.AuthenticateAsync(email);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task RegisterAsync(RegisterRequest registerRequest)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(registerRequest.Email);
            if (existingUser != null)
            {
                throw new ServiceException("User with the given email already exists.", 409);
            }
            var user = _mapper.Map<User>(registerRequest);
            try
            {
                await _userRepository.RegisterAsync(user);
            }
            catch (DbUpdateException ex)
            {
                throw new ServiceException("Register Error", 500, ex);
            }
        }
    }
}
