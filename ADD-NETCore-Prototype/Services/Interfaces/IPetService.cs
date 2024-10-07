using Api.DTOs;

namespace Api.Services.Interfaces
{
    public interface IPetService
    {
        Task<IEnumerable<PetResponseDto>> GetAllPetsAsync();
        Task<PetResponseDto> GetPetByIdAsync(string id);
        Task<PetResponseDto> AddPetAsync(PetCreateRequestDto petDto);
        Task<PetResponseDto> UpdatePetAsync(string id, PetCreateRequestDto updatedPetDto);
        Task DeletePetAsync(string id);
    }
}
