using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Controllers;
using Api.DTOs;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;

namespace Api.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                null, null, null, null
            );
            _mockMapper = new Mock<IMapper>();
            _controller = new UserController(_mockUserManager.Object, _mockRoleManager.Object, _mockMapper.Object);
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
        public async Task GetCurrentUserAsync_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            SetUser("user1");
            var user = new User { Id = "user1", UserName = "testuser" };
            var userDto = new UserResponseDto 
            { 
                Id = "user1", 
                FirstName = "Test", 
                LastName = "User", 
                Email = "testuser@example.com" 
            };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.GetCurrentUserAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userDto, okResult.Value);
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_ShouldReturnOk_WhenUserIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var user = new User { Id = "user1", UserName = "testuser" };
            var updateUserDto = new UserCreateRequestDto 
            { 
                FirstName = "Updated",
                LastName = "User"
            };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
_mockMapper.Setup(m => m.Map(updateUserDto, user)).Callback(() => 
{
    user.FirstName = updateUserDto.FirstName;
    user.LastName = updateUserDto.LastName;
});

            // Act
            var result = await _controller.UpdateCurrentUserAsync(updateUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task DeleteCurrentUserAsync_ShouldReturnNoContent_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var user = new User { Id = "user1" };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteCurrentUserAsync();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // ==================== Admin Tests ====================

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var user = new User { Id = "user1", UserName = "testuser" };
            var userDto = new UserResponseDto { Id = "user1", FirstName = "Test", LastName = "User", Email = "testuser@example.com" };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.GetUserByIdAsync("user1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userDto, okResult.Value);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUserByIdAsync("user1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnCreated_WhenUserIsValid()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var newUserDto = new UserCreateRequestDto { FirstName = "New", LastName = "User" };
            var newUser = new User { Id = "user1", FirstName = "New", LastName = "User" };
            var createdUserDto = new UserResponseDto { Id = "user1", FirstName = "New", LastName = "User", Email = "newuser@example.com" };
            _mockMapper.Setup(m => m.Map<User>(newUserDto)).Returns(newUser);
            _mockUserManager.Setup(um => um.CreateAsync(newUser)).ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(newUser)).Returns(createdUserDto);

            // Act
            var result = await _controller.CreateUserAsync(newUserDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(createdUserDto, createdResult.Value);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _controller.ModelState.AddModelError("FirstName", "Required");
            var newUserDto = new UserCreateRequestDto { FirstName = "", LastName = "User" };

            // Act
            var result = await _controller.CreateUserAsync(newUserDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ShouldReturnNoContent_WhenUserIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var user = new User { Id = "user1", UserName = "testuser" };
            var updateUserDto = new UserUpdateRequestDto { FirstName = "Updated", LastName = "User", Email = "updateduser@example.com" };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map(updateUserDto, user)).Callback(() => 
            {
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.Email = updateUserDto.Email;
            });

            // Act
            var result = await _controller.UpdateUserByIdAsync("user1", updateUserDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _controller.ModelState.AddModelError("FirstName", "Required");
            var updateUserDto = new UserUpdateRequestDto { FirstName = "", LastName = "User", Email = "updateduser@example.com" };

            // Act
            var result = await _controller.UpdateUserByIdAsync("user1", updateUserDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnNoContent_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var user = new User { Id = "user1" };
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteUserByIdAsync("user1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.DeleteUserByIdAsync("user1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
