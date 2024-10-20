using Api.Data.Interface;
using Api.DTOs;
using Api.Models;
using Api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Implementations
{
    public class PetService : BaseService, IPetService
    {
        private readonly IMapper _mapper;

        public PetService(IDbContextFactory dbContextFactory, IMapper mapper)
            : base(dbContextFactory)
        {
            _mapper = mapper;
        }

        public async Task<PetResponseDto> GetPetByIdAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var pet =
                await dbContext
                    .Pets.Include(p => p.HealthData) // Include related HealthData if necessary
                    .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Pet with Id {id} not found.");

            // Map the Pet entity to the PetResponseDto and return
            return _mapper.Map<PetResponseDto>(pet);
        }

        public async Task<PetResponseDto> AddPetAsync(PetCreateRequestDto petDto, string userId)
        {
            if (petDto == null)
                throw new ArgumentNullException(nameof(petDto), "Pet cannot be null.");

            // Ensure that the userId is not null or empty
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

            // Create the DbContext (ensure this is correct based on your setup)
            using var dbContext = await CreateDbContextAsync();

            // Map the DTO to the Pet entity
            var pet = _mapper.Map<Pet>(petDto);

            // Set the OwnerId (the authenticated user's ID)
            pet.OwnerId = userId;

            // Add the pet to the database
            await dbContext.Pets.AddAsync(pet);
            await dbContext.SaveChangesAsync();

            // Map the saved entity back to a response DTO and return it
            return _mapper.Map<PetResponseDto>(pet);
        }

        public async Task<IEnumerable<PetResponseDto>> GetAllPetsAsync()
        {
            using var dbContext = await CreateDbContextAsync();
            var pets = await dbContext.Pets.Include(p => p.HealthData).ToListAsync();

            // Map the list of Pet entities to a list of PetResponseDto
            return _mapper.Map<IEnumerable<PetResponseDto>>(pets);
        }

        public async Task DeletePetAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var pet =
                await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Pet with Id {id} not found.");

            dbContext.Pets.Remove(pet);
            await dbContext.SaveChangesAsync();
        }

        public async Task<PetResponseDto> UpdatePetAsync(
            string id,
            PetCreateRequestDto updatedPetDto
        )
        {
            if (updatedPetDto == null)
                throw new ArgumentNullException(nameof(updatedPetDto), "Pet DTO cannot be null.");

            using var dbContext = await CreateDbContextAsync();
            var pet =
                await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Pet with Id {id} not found.");

            // Map the updated DTO values to the existing Pet entity
            _mapper.Map(updatedPetDto, pet);

            await dbContext.SaveChangesAsync();

            // Return the updated pet as a response DTO
            return _mapper.Map<PetResponseDto>(pet);
        }

        public async Task<HealthDataResponseDto> AddHealthDataAsync(
            string petId,
            HealthDataCreateRequestDto healthDataDto
        )
        {
            if (healthDataDto == null)
                throw new ArgumentNullException(
                    nameof(healthDataDto),
                    "Health data DTO cannot be null."
                );

            using var dbContext = await CreateDbContextAsync();

            var pet =
                await dbContext
                    .Pets.Include(p => p.HealthData)
                    .FirstOrDefaultAsync(p => p.Id == petId)
                ?? throw new KeyNotFoundException($"Pet with Id {petId} not found.");

            // Create and map the HealthData
            var healthData = _mapper.Map<HealthData>(healthDataDto);
            pet.HealthData = healthData;

            await dbContext.SaveChangesAsync();

            // Return the added health data
            return _mapper.Map<HealthDataResponseDto>(healthData);
        }

        public async Task DeleteHealthDataAsync(string petId, string healthDataId)
        {
            using var dbContext = await CreateDbContextAsync();

            var pet =
                await dbContext
                    .Pets.Include(p => p.HealthData)
                    .FirstOrDefaultAsync(p => p.Id == petId)
                ?? throw new KeyNotFoundException($"Pet with Id {petId} not found.");

            if (pet.HealthData == null || pet.HealthData.Id != healthDataId)
                throw new KeyNotFoundException($"Health data with Id {healthDataId} not found.");

            // Remove HealthData
            pet.HealthData = null;
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<PetResponseDto>> GetUserPetsAsync(string userId)
        {
            using var dbContext = await CreateDbContextAsync();
            var pets = await dbContext
                .Pets.Where(p => p.OwnerId == userId)
                .Include(p => p.HealthData)
                .ToListAsync();

            // Map the list of Pet entities to a list of PetResponseDto
            return _mapper.Map<IEnumerable<PetResponseDto>>(pets);
        }
    }
}
