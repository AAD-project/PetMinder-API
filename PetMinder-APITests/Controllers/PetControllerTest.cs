using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Controllers;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;

namespace Api.Tests.Controllers
{
    public class PetControllerTests
    {
        private readonly Mock<IPetService> _mockPetService;
        private readonly PetController _controller;

        public PetControllerTests()
        {
            _mockPetService = new Mock<IPetService>();
            _controller = new PetController(_mockPetService.Object);
        }

        private void SetUser(string userId, string role = null)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ==================== Regular User Tests ====================

        [Fact]
        public async Task GetUserPetsAsync_ShouldReturnOk_WhenPetsExist()
        {
            // Arrange
            SetUser("user1");
            var pets = new List<PetResponseDto> { new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user1" } };
            _mockPetService.Setup(s => s.GetUserPetsAsync("user1")).ReturnsAsync(pets);

            // Act
            var result = await _controller.GetUserPetsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(pets, okResult.Value);
        }

        [Fact]
        public async Task GetUserPetsAsync_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            SetUser("user1");
            _mockPetService.Setup(s => s.GetUserPetsAsync("user1")).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.GetUserPetsAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetUserPetByIdAsync_ShouldReturnOk_WhenPetExistsAndUserOwnsIt()
        {
            // Arrange
            SetUser("user1");
            var pet = new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(pet);

            // Act
            var result = await _controller.GetUserPetByIdAsync("pet1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(pet, okResult.Value);
        }

        [Fact]
        public async Task GetUserPetByIdAsync_ShouldReturnNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _controller.GetUserPetByIdAsync("pet1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserPetByIdAsync_ShouldReturnForbid_WhenUserDoesNotOwnPet()
        {
            // Arrange
            SetUser("user1");
            var pet = new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user2" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(pet);

            // Act
            var result = await _controller.GetUserPetByIdAsync("pet1");

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task AddUserPetAsync_ShouldReturnBadRequest_WhenPetIsNull()
        {
            // Arrange
            SetUser("user1");

            // Act
            var result = await _controller.AddUserPetAsync(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateUserPetAsync_ShouldReturnNoContent_WhenPetIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var updatedPet = new PetCreateRequestDto { Name = "Updated Buddy", DateOfBirth = new DateTime(2019, 5, 5), Type = "Dog", Breed = "Golden Retriever", Gender = "Female", Weight = 30.0f };
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.UpdateUserPetAsync("pet1", updatedPet);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserPetAsync_ShouldReturnNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            var updatedPet = new PetCreateRequestDto { Name = "Updated Buddy" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _controller.UpdateUserPetAsync("pet1", updatedPet);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserPetAsync_ShouldReturnForbid_WhenUserDoesNotOwnPet()
        {
            // Arrange
            SetUser("user1");
            var updatedPet = new PetCreateRequestDto { Name = "Updated Buddy" };
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user2" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.UpdateUserPetAsync("pet1", updatedPet);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteUserPetAsync_ShouldReturnNoContent_WhenPetIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.DeleteUserPetAsync("pet1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserPetAsync_ShouldReturnNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _controller.DeleteUserPetAsync("pet1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUserPetAsync_ShouldReturnForbid_WhenUserDoesNotOwnPet()
        {
            // Arrange
            SetUser("user1");
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user2" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.DeleteUserPetAsync("pet1");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        // ==================== Admin Tests ====================

        [Fact]
        public async Task GetAllPetsAsync_ShouldReturnOk_WhenPetsExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var pets = new List<PetResponseDto> { new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user1" } };
            _mockPetService.Setup(s => s.GetAllPetsAsync()).ReturnsAsync(pets);

            // Act
            var result = await _controller.GetAllPetsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(pets, okResult.Value);
        }

        [Fact]
        public async Task GetAllPetsAsync_ShouldReturn500_WhenExceptionIsThrown()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockPetService.Setup(s => s.GetAllPetsAsync()).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.GetAllPetsAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetPetByIdForAdminAsync_ShouldReturnOk_WhenPetExists()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var pet = new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(pet);

            // Act
            var result = await _controller.GetPetByIdForAdminAsync("pet1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(pet, okResult.Value);
        }

        [Fact]
        public async Task GetPetByIdForAdminAsync_ShouldReturnNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync((PetResponseDto)null);

            // Act
            var result = await _controller.GetPetByIdForAdminAsync("pet1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddPetForUserAsync_ShouldReturnCreated_WhenPetIsAddedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var newPet = new PetCreateRequestDto { Name = "Buddy", OwnerId = "user1", DateOfBirth = new DateTime(2020, 1, 1), Type = "Dog", Breed = "Labrador", Gender = "Male", Weight = 25.0f };
            var addedPet = new PetResponseDto { Id = "pet1", Name = "Buddy", OwnerId = "user1", DateOfBirth = "2020-01-01", Type = "Dog", Breed = "Labrador", Gender = "Male", Weight = 25.0f };
            _mockPetService.Setup(s => s.AddPetAsync(newPet, "user1")).ReturnsAsync(addedPet);

            // Act
            var result = await _controller.AddPetForUserAsync("user1", newPet);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(addedPet, createdResult.Value);
        }

        [Fact]
        public async Task UpdatePetForAdminAsync_ShouldReturnNoContent_WhenPetIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var updatedPet = new PetCreateRequestDto { Name = "Updated Buddy", DateOfBirth = new DateTime(2019, 5, 5), Type = "Dog", Breed = "Golden Retriever", Gender = "Female", Weight = 30.0f };
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.UpdatePetForAdminAsync("pet1", updatedPet);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePetForAdminAsync_ShouldReturnNoContent_WhenPetIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var existingPet = new PetResponseDto { Id = "pet1", OwnerId = "user1" };
            _mockPetService.Setup(s => s.GetPetByIdAsync("pet1")).ReturnsAsync(existingPet);

            // Act
            var result = await _controller.DeletePetForAdminAsync("pet1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
