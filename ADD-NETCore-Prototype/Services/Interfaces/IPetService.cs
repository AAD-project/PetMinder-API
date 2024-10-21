using Api.DTOs;

namespace Api.Services.Interfaces
{
    public interface IPetService
    {
        Task<IEnumerable<PetResponseDto>> GetAllPetsAsync();
        Task<PetResponseDto> GetPetByIdAsync(string id);
        Task<PetResponseDto> AddPetAsync(PetCreateRequestDto petDto, string userId);
        Task<PetResponseDto> UpdatePetAsync(string id, PetCreateRequestDto updatedPetDto);
        Task DeletePetAsync(string id);
        Task<HealthDataResponseDto> AddHealthDataAsync(
            string petId,
            HealthDataCreateRequestDto healthDataDto
        );
        Task DeleteHealthDataAsync(string petId, string healthDataId);

        Task<IEnumerable<PetResponseDto>> GetUserPetsAsync(string userId);
    }
}
