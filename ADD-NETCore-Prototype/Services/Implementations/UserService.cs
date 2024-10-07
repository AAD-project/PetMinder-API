using Api.Data.Interface;
using Api.DTOs;
using Api.Models;
using Api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Implementations
{
    public class UserService : BaseService, IUserService
    {
        private readonly IMapper _mapper;

        public UserService(IDbContextFactory dbContextFactory, IMapper mapper)
            : base(dbContextFactory)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            using var dbContext = await CreateDbContextAsync();
            var users = await dbContext
                .Users.Include(u => u.Pets)
                .Include(u => u.Tasks)
                .Include(u => u.Reminders)
                .ToListAsync();

            // Map list of User entities to list of UserResponseDto
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var user =
                await dbContext
                    .Users.Include(u => u.Pets)
                    .Include(u => u.Tasks)
                    .Include(u => u.Reminders)
                    .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException($"User with Id {id} not found.");

            // Map the User entity to UserResponseDto
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> AddUserAsync(UserCreateRequestDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto), "User data cannot be null.");

            using var dbContext = await CreateDbContextAsync();

            // Map the incoming UserCreateRequestDto to the User entity
            var user = _mapper.Map<User>(userDto);
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            // Return the newly created user as a response DTO
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdateUserAsync(
            string id,
            UserCreateRequestDto updatedUserDto
        )
        {
            if (updatedUserDto == null)
                throw new ArgumentNullException(
                    nameof(updatedUserDto),
                    "Updated user data cannot be null."
                );

            using var dbContext = await CreateDbContextAsync();
            var existingUser =
                await dbContext
                    .Users.Include(u => u.Pets)
                    .Include(u => u.Tasks)
                    .Include(u => u.Reminders)
                    .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException($"User with Id {id} not found.");

            // Map the updated DTO values to the existing User entity
            _mapper.Map(updatedUserDto, existingUser);

            await dbContext.SaveChangesAsync();

            // Return the updated user as a response DTO
            return _mapper.Map<UserResponseDto>(existingUser);
        }

        public async Task DeleteUserAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var user =
                await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException($"User with Id {id} not found.");
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<PetResponseDto>> GetAllPetsFromUserAsync(string ownerId)
        {
            using var dbContext = await CreateDbContextAsync();
            var user =
                await dbContext.Users.Include(u => u.Pets).FirstOrDefaultAsync(u => u.Id == ownerId)
                ?? throw new KeyNotFoundException($"User with Id {ownerId} not found.");

            // Map the list of Pet entities to a list of PetResponseDto
            return _mapper.Map<IEnumerable<PetResponseDto>>(user.Pets);
        }
    }
}
