using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;

namespace Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> GetUserByIdAsync(string id);
        Task<UserResponseDto> AddUserAsync(UserCreateRequestDto userDto);
        Task<UserResponseDto> UpdateUserAsync(string id, UserCreateRequestDto updatedUserDto);
        Task DeleteUserAsync(string id);
        Task<IEnumerable<PetResponseDto>> GetAllPetsFromUserAsync(string ownerId);
    }
}
