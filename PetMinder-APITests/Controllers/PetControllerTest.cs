using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Api.Tests.Controllers
{
    public class PetControllerTest
    {
        private readonly Mock<IPetService> _mockPetService;
        private readonly PetController _petController;

        public PetControllerTest()
        {
            _mockPetService = new Mock<IPetService>();
            _petController = new PetController(_mockPetService.Object);
        }

        [Fact]
        public async Task GetAllPetsAsync_ReturnsOkResult_WithListOfPets()
        {
            // Arrange
            var pets = new List<PetResponseDto> { new PetResponseDto { Id = "1", Name = "Buddy" } };
            _mockPetService.Setup(service => service.GetAllPetsAsync()).ReturnsAsync(pets);

            // Act
            var result = await _petController.GetAllPetsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<PetResponseDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetAllPetsAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetAllPetsAsync()).ThrowsAsync(new Exception());

            // Act
            var result = await _petController.GetAllPetsAsync();

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, (result.Result as ObjectResult).StatusCode);
        }

        [Fact]
        public async Task GetPetByIdAsync_ReturnsOkResult_WithPet()
        {
            // Arrange
            var pet = new PetResponseDto { Id = "1", Name = "Buddy" };
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync(pet);

            // Act
            var result = await _petController.GetPetByIdAsync("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PetResponseDto>(okResult.Value);
            Assert.Equal("Buddy", returnValue.Name);
        }

        [Fact]
        public async Task GetPetByIdAsync_ReturnsNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _petController.GetPetByIdAsync("1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPetByIdAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ThrowsAsync(new Exception());

            // Act
            var result = await _petController.GetPetByIdAsync("1");

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, (result.Result as ObjectResult).StatusCode);
        }

        [Fact]
        public async Task AddPetAsync_ReturnsCreatedAtActionResult_WithCreatedPet()
        {
            // Arrange
            var newPet = new PetCreateRequestDto { Name = "Buddy" };
            var createdPet = new PetResponseDto { Id = "1", Name = "Buddy" };
            _mockPetService.Setup(service => service.AddPetAsync(newPet)).ReturnsAsync(createdPet);

            // Act
            var result = await _petController.AddPetAsync(newPet);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<PetResponseDto>(createdAtActionResult.Value);
            Assert.Equal("Buddy", returnValue.Name);
        }

        [Fact]
        public async Task AddPetAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _petController.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _petController.AddPetAsync(new PetCreateRequestDto());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddPetAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var newPet = new PetCreateRequestDto { Name = "Buddy" };
            _mockPetService.Setup(service => service.AddPetAsync(newPet)).ThrowsAsync(new Exception());

            // Act
            var result = await _petController.AddPetAsync(newPet);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, (result.Result as ObjectResult).StatusCode);
        }

        [Fact]
        public async Task UpdatePetAsync_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updatedPet = new PetCreateRequestDto { Name = "Buddy" };
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync(new PetResponseDto { Id = "1", Name = "Buddy" });
            _mockPetService.Setup(service => service.UpdatePetAsync("1", updatedPet)).ReturnsAsync(new PetResponseDto { Id = "1", Name = "Buddy" });

            // Act
            var result = await _petController.UpdatePetAsync("1", updatedPet);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdatePetAsync_ReturnsNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            var updatedPet = new PetCreateRequestDto { Name = "Buddy" };
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _petController.UpdatePetAsync("1", updatedPet);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdatePetAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _petController.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _petController.UpdatePetAsync("1", new PetCreateRequestDto());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePetAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var updatedPet = new PetCreateRequestDto { Name = "Buddy" };
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync(new PetResponseDto { Id = "1", Name = "Buddy" });
            _mockPetService.Setup(service => service.UpdatePetAsync("1", updatedPet)).ThrowsAsync(new Exception());

            // Act
            var result = await _petController.UpdatePetAsync("1", updatedPet);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }


        [Fact]
        public async Task DeletePetAsync_ReturnsNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync(new PetResponseDto { Id = "1", Name = "Buddy" });
            _mockPetService.Setup(service => service.DeletePetAsync("1")).Returns(Task.CompletedTask);

            // Act
            var result = await _petController.DeletePetAsync("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePetAsync_ReturnsNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _petController.DeletePetAsync("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePetAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockPetService.Setup(service => service.GetPetByIdAsync("1")).ReturnsAsync(new PetResponseDto { Id = "1", Name = "Buddy" });
            _mockPetService.Setup(service => service.DeletePetAsync("1")).ThrowsAsync(new Exception());

            // Act
            var result = await _petController.DeletePetAsync("1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

    }
}