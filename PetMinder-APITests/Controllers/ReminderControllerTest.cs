using Api.Controllers;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetMinder_APITests.Controllers
{
   public class ReminderControllerTest
    {
        private readonly Mock<IReminderService> _mockReminderService;
        private readonly ReminderController _controller;

        public ReminderControllerTest()
        {
            _mockReminderService = new Mock<IReminderService>();
            _controller = new ReminderController(_mockReminderService.Object);
        }

        [Fact]
        public async Task CreateReminderAsync_ReturnsCreatedAtActionResult_WhenReminderIsValid()
        {
            // Arrange
            var reminderDto = new ReminderCreateRequestDto
            {
                UserId = "user1",
                PetId = "pet1",
                Title = "Reminder Title"
                // Initialize other properties if needed
            };
            var reminderResponseDto = new ReminderResponseDto { Id = "1", UserId = "user1", Title = "Reminder Title" /* Initialize other properties if needed */ };

            _mockReminderService
                .Setup(service => service.CreateReminderAsync(reminderDto))
                .ReturnsAsync(reminderResponseDto);

            // Act
            var result = await _controller.CreateReminderAsync(reminderDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ReminderResponseDto>(actionResult.Value);
            Assert.Equal(reminderResponseDto.Id, returnValue.Id);
        }

        [Fact]
        public async Task CreateReminderAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.CreateReminderAsync(new ReminderCreateRequestDto
            {
                UserId = "user1",
                PetId = "pet1",
                Title = "Reminder Title"
            });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateReminderAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var reminderDto = new ReminderCreateRequestDto
            {
                UserId = "user1",
                PetId = "pet1",
                Title = "Reminder Title"
            };
            _mockReminderService.Setup(service => service.CreateReminderAsync(reminderDto)).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.CreateReminderAsync(reminderDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetReminderByIdAsync_ReturnsOkObjectResult_WhenReminderExists()
        {
            // Arrange
            var reminderId = "1";
            var reminderResponseDto = new ReminderResponseDto { Id = reminderId, UserId = "user1", Title = "Reminder Title" /* Initialize other properties if needed */ };

            _mockReminderService
                .Setup(service => service.GetReminderByIdAsync(reminderId))
                .ReturnsAsync(reminderResponseDto);

            // Act
            var result = await _controller.GetReminderByIdAsync(reminderId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ReminderResponseDto>(actionResult.Value);
            Assert.Equal(reminderId, returnValue.Id);
        }

        [Fact]
        public async Task GetReminderByIdAsync_ReturnsNotFoundResult_WhenReminderDoesNotExist()
        {
            // Arrange
            var reminderId = "1";

            _mockReminderService
                .Setup(service => service.GetReminderByIdAsync(reminderId))
                .ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.GetReminderByIdAsync(reminderId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetReminderByIdAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockReminderService.Setup(service => service.GetReminderByIdAsync("1")).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.GetReminderByIdAsync("1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

       [Fact]
        public async Task CreateReminderAsync_ReturnsBadRequest_WhenReminderDtoIsNull()
        {
            // Arrange
            var reminderDto = (ReminderCreateRequestDto)null;

            // Act
            var result = await _controller.CreateReminderAsync(reminderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Reminder data is invalid.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRemindersByUserIdAsync_ReturnsEmptyList_WhenNoRemindersExist()
        {
            // Arrange
            var userId = "user1";
            _mockReminderService.Setup(service => service.GetAllRemindersByUserIdAsync(userId)).ReturnsAsync(new List<ReminderResponseDto>());

            // Act
            var result = await _controller.GetAllRemindersByUserIdAsync(userId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<ReminderResponseDto>>(actionResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetAllRemindersByUserIdAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var userId = "user1";
            _mockReminderService.Setup(service => service.GetAllRemindersByUserIdAsync(userId)).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.GetAllRemindersByUserIdAsync(userId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateReminderAsync_ReturnsNoContentResult_WhenReminderIsUpdated()
        {
            // Arrange
            var reminderId = "1";
            var reminderDto = new ReminderCreateRequestDto
            {
                UserId = "user1",
                PetId = "pet1",
                Title = "Reminder Title"
                // Initialize other properties if needed
            };
            var reminderResponseDto = new ReminderResponseDto { Id = reminderId, UserId = "user1", Title = "Reminder Title" /* Initialize other properties if needed */ };

            _mockReminderService
                .Setup(service => service.GetReminderByIdAsync(reminderId))
                .ReturnsAsync(reminderResponseDto);

            _mockReminderService
                .Setup(service => service.UpdateReminderAsync(reminderId, reminderDto))
                .ReturnsAsync(reminderResponseDto);

            // Act
            var result = await _controller.UpdateReminderAsync(reminderId, reminderDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateReminderAsync_ReturnsNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            var reminderDto = new ReminderCreateRequestDto { UserId = "user1", PetId = "pet1", Title = "Reminder Title" };
            _mockReminderService.Setup(service => service.GetReminderByIdAsync("1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.UpdateReminderAsync("1", reminderDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateReminderAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.UpdateReminderAsync("1", new ReminderCreateRequestDto
            {
                UserId = "user1",
                PetId = "pet1",
                Title = "Reminder Title"
            });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateReminderAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var reminderDto = new ReminderCreateRequestDto { UserId = "user1", PetId = "pet1", Title = "Reminder Title" };
            _mockReminderService.Setup(service => service.GetReminderByIdAsync("1")).ReturnsAsync(new ReminderResponseDto { Id = "1", UserId = "user1", Title = "Reminder Title" });
            _mockReminderService.Setup(service => service.UpdateReminderAsync("1", reminderDto)).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.UpdateReminderAsync("1", reminderDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteReminderAsync_ReturnsNoContentResult_WhenReminderIsDeleted()
        {
            // Arrange
            var reminderId = "1";
            var reminderResponseDto = new ReminderResponseDto { Id = reminderId, UserId = "user1", Title = "Reminder Title" /* Initialize other properties if needed */ };

            _mockReminderService
                .Setup(service => service.GetReminderByIdAsync(reminderId))
                .ReturnsAsync(reminderResponseDto);

            _mockReminderService
                .Setup(service => service.DeleteReminderAsync(reminderId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteReminderAsync(reminderId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteReminderAsync_ReturnsNotFound_WhenReminderDoesNotExist()
        {
            // Arrange
            _mockReminderService.Setup(service => service.GetReminderByIdAsync("1")).ReturnsAsync((ReminderResponseDto)null);

            // Act
            var result = await _controller.DeleteReminderAsync("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteReminderAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockReminderService.Setup(service => service.GetReminderByIdAsync("1")).ReturnsAsync(new ReminderResponseDto { Id = "1", UserId = "user1", Title = "Reminder Title" });
            _mockReminderService.Setup(service => service.DeleteReminderAsync("1")).ThrowsAsync(new System.Exception());

            // Act
            var result = await _controller.DeleteReminderAsync("1");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
