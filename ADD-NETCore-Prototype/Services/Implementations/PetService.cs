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

        public async Task<PetResponseDto> AddPetAsync(PetCreateRequestDto petDto)
        {
            if (petDto == null)
                throw new ArgumentNullException(nameof(petDto), "Pet DTO cannot be null.");

            using var dbContext = await CreateDbContextAsync();

            // Map the incoming DTO to the Pet entity
            var pet = _mapper.Map<Pet>(petDto);

            await dbContext.Pets.AddAsync(pet);
            await dbContext.SaveChangesAsync();

            // Return the created pet as a response DTO
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
    }
}
