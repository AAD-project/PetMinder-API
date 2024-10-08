using Api.Controllers;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _userController;

        public UserControllerTest()
        {
            _mockUserService = new Mock<IUserService>();
            _userController = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsOkResult()
        {
            // Arrange
            var userId = "1";
            var userResponseDto = new UserResponseDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com" 
            };
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(userResponseDto);

            // Act
            var result = await _userController.GetUserByIdAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserResponseDto>(okResult.Value);
            Assert.Equal(userId, returnValue.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "1";
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto)null);

            // Act
            var result = await _userController.GetUserByIdAsync(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_UserExists_ReturnsNoContentResult()
        {
            // Arrange
            var userId = "1";
            var userResponseDto = new UserResponseDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com" 
            };
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(userResponseDto);
            _mockUserService.Setup(service => service.DeleteUserAsync(userId))
                .Returns(Task.FromResult((UserResponseDto)null));

            // Act
            var result = await _userController.DeleteUserByIdAsync(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_UserDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "1";
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto)null);

            // Act
            var result = await _userController.DeleteUserByIdAsync(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUserInfoAsync_UserExists_ReturnsNoContentResult()
        {
            // Arrange
            var userId = "1";
            var userResponseDto = new UserResponseDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com" 
            };
            var userUpdateDto = new UserCreateRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(userResponseDto);
            _mockUserService.Setup(service => service.UpdateUserAsync(userId, userUpdateDto))
                .ReturnsAsync(userResponseDto);

            // Act
            var result = await _userController.UpdateUserInfoAsync(userId, userUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserInfoAsync_UserDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "1";
            var userUpdateDto = new UserCreateRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto)null);

            // Act
            var result = await _userController.UpdateUserInfoAsync(userId, userUpdateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateUserAsync_ValidUser_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var userId = "1";
            var userCreateDto = new UserCreateRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
            var userResponseDto = new UserResponseDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com" 
            };
            
            _mockUserService.Setup(service => service.AddUserAsync(userCreateDto))
                .ReturnsAsync(userResponseDto);

            // Act
            var result = await _userController.CreateUserAsync(userCreateDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<UserResponseDto>(createdAtActionResult.Value);
            Assert.Equal(userId, returnValue.Id);
        }

        [Fact]
        public async Task GetAllPetsFromUserAsync_UserExists_ReturnsOkResult()
        {
            // Arrange
            var userId = "1";
            var userResponseDto = new UserResponseDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com" 
            };
            var pets = new List<PetResponseDto> { new PetResponseDto { Id = "1" } };
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(userResponseDto);
            _mockUserService.Setup(service => service.GetAllPetsFromUserAsync(userId))
                .ReturnsAsync(pets);

            // Act
            var result = await _userController.GetAllPetsFromUserAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<PetResponseDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetAllPetsFromUserAsync_UserDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var userId = "1";
            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto)null);

            // Act
            var result = await _userController.GetAllPetsFromUserAsync(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}