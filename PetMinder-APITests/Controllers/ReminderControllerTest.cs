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
    public class ReminderControllerTests
    {
        private readonly Mock<IReminderService> _mockReminderService;
        private readonly ReminderController _controller;

        public ReminderControllerTests()
        {
            _mockReminderService = new Mock<IReminderService>();
            _controller = new ReminderController(_mockReminderService.Object);
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
        public async Task CreateUserReminderAsync_ShouldReturnCreated_WhenReminderIsValid()
        {
            // Arrange
            SetUser("user1");
            var newReminder = new ReminderCreateRequestDto { Title = "Vet Appointment", ReminderDateTime = DateTime.Now.AddDays(1), UserId = "user1", PetId = "pet1" };
            var createdReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1", ReminderDateTime = DateTime.Now.AddDays(1) };
            _mockReminderService.Setup(s => s.CreateReminderAsync(newReminder)).ReturnsAsync(createdReminder);

            // Act
            var result = await _controller.CreateUserReminderAsync(newReminder);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = createdResult.Value as ReminderResponseDto;
            Assert.NotNull(response);
            Assert.Equal(createdReminder.Id, response.Id);
            Assert.Equal(createdReminder.Title, response.Title);
            Assert.Equal(createdReminder.UserId, response.UserId);
        }

        [Fact]
        public async Task CreateUserReminderAsync_ShouldReturnBadRequest_WhenReminderIsNull()
        {
            // Arrange
            SetUser("user1");

            // Act
            var result = await _controller.CreateUserReminderAsync(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserReminderByIdAsync_ShouldReturnOk_WhenReminderExistsAndUserOwnsIt()
        {
            // Arrange
            SetUser("user1");
            var reminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(reminder);

            // Act
            var result = await _controller.GetUserReminderByIdAsync("reminder1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reminder, okResult.Value);
        }

        [Fact]
        public async Task GetUserReminderByIdAsync_ShouldReturnNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.GetUserReminderByIdAsync("reminder1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUserReminderByIdAsync_ShouldReturnForbid_WhenUserDoesNotOwnReminder()
        {
            // Arrange
            SetUser("user1");
            var reminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user2" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(reminder);

            // Act
            var result = await _controller.GetUserReminderByIdAsync("reminder1");

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetAllUserRemindersAsync_ShouldReturnOk_WhenRemindersExist()
        {
            // Arrange
            SetUser("user1");
            var reminders = new List<ReminderResponseDto> { new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" } };
            _mockReminderService.Setup(s => s.GetAllRemindersByUserIdAsync("user1")).ReturnsAsync(reminders);

            // Act
            var result = await _controller.GetAllUserRemindersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reminders, okResult.Value);
        }

        [Fact]
        public async Task UpdateUserReminderAsync_ShouldReturnNoContent_WhenReminderIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var updatedReminder = new ReminderCreateRequestDto { Title = "Updated Vet Appointment", UserId = "user1", PetId = "pet1" };
            var existingReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.UpdateUserReminderAsync("reminder1", updatedReminder);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserReminderAsync_ShouldReturnNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            var updatedReminder = new ReminderCreateRequestDto { Title = "Updated Vet Appointment", UserId = "user1", PetId = "pet1" };

            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.UpdateUserReminderAsync("reminder1", updatedReminder);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUserReminderAsync_ShouldReturnForbid_WhenUserDoesNotOwnReminder()
        {
            // Arrange
            SetUser("user1");
            var updatedReminder = new ReminderCreateRequestDto { Title = "Updated Vet Appointment", UserId = "user1", PetId = "pet1" };
            var existingReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user2" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.UpdateUserReminderAsync("reminder1", updatedReminder);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteUserReminderAsync_ShouldReturnNoContent_WhenReminderIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var existingReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.DeleteUserReminderAsync("reminder1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserReminderAsync_ShouldReturnNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.DeleteUserReminderAsync("reminder1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUserReminderAsync_ShouldReturnForbid_WhenUserDoesNotOwnReminder()
        {
            // Arrange
            SetUser("user1");
            var existingReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user2" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.DeleteUserReminderAsync("reminder1");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        // ==================== Admin Tests ====================

        [Fact]
        public async Task GetAllRemindersByUserIdForAdminAsync_ShouldReturnOk_WhenRemindersExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var reminders = new List<ReminderResponseDto> { new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" } };
            _mockReminderService.Setup(s => s.GetAllRemindersByUserIdAsync("user1")).ReturnsAsync(reminders);

            // Act
            var result = await _controller.GetAllRemindersByUserIdForAdminAsync("user1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reminders, okResult.Value);
        }

        [Fact]
        public async Task GetReminderByIdForAdminAsync_ShouldReturnOk_WhenReminderExists()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var reminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(reminder);

            // Act
            var result = await _controller.GetReminderByIdForAdminAsync("reminder1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reminder, okResult.Value);
        }

        [Fact]
        public async Task GetReminderByIdForAdminAsync_ShouldReturnNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.GetReminderByIdForAdminAsync("reminder1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateReminderForUserAsync_ShouldReturnCreated_WhenReminderIsValid()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var newReminder = new ReminderCreateRequestDto { Title = "Vet Appointment", UserId = "user1", PetId = "pet1", ReminderDateTime = DateTime.Now.AddDays(1) };
            var createdReminder = new ReminderResponseDto { Id = "reminder1", Title = "Vet Appointment", UserId = "user1", NextReminderDateTimeList = new List<DateTime> { DateTime.Now.AddDays(1) } };
            _mockReminderService.Setup(s => s.CreateReminderAsync(newReminder)).ReturnsAsync(createdReminder);

            // Act
            var result = await _controller.CreateReminderForUserAsync(newReminder);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = createdResult.Value as ReminderResponseDto;
            Assert.NotNull(response);
            Assert.Equal(createdReminder.Id, response.Id);
            Assert.Equal(createdReminder.Title, response.Title);
            Assert.Equal(createdReminder.UserId, response.UserId);
        }

        [Fact]
        public async Task UpdateReminderForAdminAsync_ShouldReturnNoContent_WhenReminderIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var updatedReminder = new ReminderCreateRequestDto { Title = "Updated Vet Appointment", ReminderDateTime = DateTime.Now.AddDays(2), UserId = "user1", PetId = "pet1" };
            var existingReminder = new ReminderResponseDto { Id = "reminder1", UserId = "user1", Title = "Vet Appointment" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.UpdateReminderForAdminAsync("reminder1", updatedReminder);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteReminderForAdminAsync_ShouldReturnNoContent_WhenReminderIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var existingReminder = new ReminderResponseDto { Id = "reminder1", UserId = "user1", Title = "Vet Appointment" };
            _mockReminderService.Setup(s => s.GetReminderByIdAsync("reminder1")).ReturnsAsync(existingReminder);

            // Act
            var result = await _controller.DeleteReminderForAdminAsync("reminder1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
