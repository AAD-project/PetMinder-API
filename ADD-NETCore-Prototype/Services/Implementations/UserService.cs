using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        // Get all users
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userManager
                .Users.Include(u => u.Pets)
                .Include(u => u.Tasks)
                .Include(u => u.Reminders)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        // Get user by ID
        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager
                .Users.Include(u => u.Pets)
                .Include(u => u.Tasks)
                .Include(u => u.Reminders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException($"User with Id {id} not found.");

            return _mapper.Map<UserResponseDto>(user);
        }

        // Add new user (Use Identity's CreateAsync for user creation)
        public async Task<UserResponseDto> AddUserAsync(UserCreateRequestDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors));
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        // Update user information
        public async Task<UserResponseDto> UpdateUserAsync(
            string id,
            UserCreateRequestDto updatedUserDto
        )
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with Id {id} not found.");

            // Update user properties
            _mapper.Map(updatedUserDto, existingUser);

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors));
            }

            return _mapper.Map<UserResponseDto>(existingUser);
        }

        // Delete user using UserManager
        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with Id {id} not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors));
            }
        }

        // Get all pets for a user
        public async Task<IEnumerable<PetResponseDto>> GetAllPetsFromUserAsync(string ownerId)
        {
            var user = await _userManager
                .Users.Include(u => u.Pets)
                .FirstOrDefaultAsync(u => u.Id == ownerId);

            if (user == null)
                throw new KeyNotFoundException($"User with Id {ownerId} not found.");

            return _mapper.Map<IEnumerable<PetResponseDto>>(user.Pets);
        }
    }
}
