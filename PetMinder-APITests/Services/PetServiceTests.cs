using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Controllers;
using Api.DTOs;
using Api.Models;
using Api.Services.Implementations;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using Api.Data.Interface;
using Api.Data.Implementations;
using System.Threading;
using System.Linq;

namespace Api.Tests.Services
{
    public class PetServiceTests
    {
        private readonly Mock<IDbContextFactory> _mockDbContextFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly PetService _petService;
        private readonly DbContextOptions<PetMinderDbContext> _options;

        public PetServiceTests()
        {
            // Set up in-memory database
            _options = new DbContextOptionsBuilder<PetMinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique DB for each test
                .Options;

            _mockDbContextFactory = new Mock<IDbContextFactory>();
            _mockDbContextFactory.Setup(f => f.CreateDbContext()).Returns(() => new PetMinderDbContext(_options));
            _mockMapper = new Mock<IMapper>();

            _petService = new PetService(_mockDbContextFactory.Object, _mockMapper.Object);
        }


        [Fact]
        public async Task GetPetByIdAsync_ShouldThrowKeyNotFoundException_WhenPetDoesNotExist()
        {
            // Arrange
            var petId = "non_existing_pet";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _petService.GetPetByIdAsync(petId));
        }

        [Fact]
        public async Task AddPetAsync_ShouldAddPet_WhenDataIsValid()
        {
            // Arrange
            var userId = "user1";
            var petDto = new PetCreateRequestDto { Name = "Buddy" };
            var pet = new Pet 
            { 
                Id = Guid.NewGuid().ToString(), 
                Name = "Buddy", 
                OwnerId = userId,
                Gender = "Male",
                Type = "Dog",
                DateOfBirth = new DateTime(2020, 1, 1),
                Breed = "Labrador"
            };
            var petResponseDto = new PetResponseDto { Id = pet.Id, Name = "Buddy", OwnerId = userId };

            _mockMapper.Setup(m => m.Map<Pet>(petDto)).Returns(pet);
            _mockMapper.Setup(m => m.Map<PetResponseDto>(pet)).Returns(petResponseDto);

            // Act
            var result = await _petService.AddPetAsync(petDto, userId);

            // Assert
            Assert.Equal(petResponseDto, result);
        }

        [Fact]
        public async Task AddPetAsync_ShouldThrowArgumentNullException_WhenPetDtoIsNull()
        {
            // Arrange
            PetCreateRequestDto petDto = null;
            var userId = "user1";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _petService.AddPetAsync(petDto, userId));
        }
 
        [Fact]
        public async Task UpdatePetAsync_ShouldThrowKeyNotFoundException_WhenPetDoesNotExist()
        {
            // Arrange
            var petId = "non_existing_pet";
            var updatedPetDto = new PetCreateRequestDto { Name = "Updated Buddy" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _petService.UpdatePetAsync(petId, updatedPetDto));
        }

        [Fact]
        public async Task GetUserPetsAsync_ShouldReturnPets_WhenPetsExistForUser()
        {
            // Arrange
            var userId = "user1";
            var pets = new List<Pet> 
            { 
                new Pet 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    OwnerId = userId, 
                    Name = "Buddy", 
                    Gender = "Male",
                    Type = "Dog",
                    DateOfBirth = new DateTime(2020, 1, 1),
                    Breed = "Labrador"
                } 
            };
            var petDtos = pets.Select(p => new PetResponseDto { Id = p.Id, OwnerId = userId, Name = p.Name }).ToList();

            using (var context = new PetMinderDbContext(_options))
            {
                context.Pets.AddRange(pets);
                await context.SaveChangesAsync();
            }
            _mockMapper.Setup(m => m.Map<IEnumerable<PetResponseDto>>(It.IsAny<List<Pet>>())).Returns(petDtos);

            // Act
            var result = await _petService.GetUserPetsAsync(userId);

            // Assert
            Assert.Equal(petDtos, result);
        }
    }
}
