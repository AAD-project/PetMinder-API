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
    public class TodoTaskControllerTests
    {
        private readonly Mock<ITodoTaskService> _mockTodoTaskService;
        private readonly TodoTaskController _controller;

        public TodoTaskControllerTests()
        {
            _mockTodoTaskService = new Mock<ITodoTaskService>();
            _controller = new TodoTaskController(_mockTodoTaskService.Object);
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
        public async Task GetUserTasksAsync_ShouldReturnOk_WhenTasksExist()
        {
            // Arrange
            SetUser("user1");
            var tasks = new List<TodoTaskResponseDto> { new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" } };
            _mockTodoTaskService.Setup(s => s.GetUserTasksAsync("user1")).ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetUserTasksAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tasks, okResult.Value);
        }

        [Fact]
        public async Task GetUserTaskByIdAsync_ShouldReturnOk_WhenTaskExistsAndUserOwnsIt()
        {
            // Arrange
            SetUser("user1");
            var task = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(task);

            // Act
            var result = await _controller.GetUserTaskByIdAsync("task1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(task, okResult.Value);
        }

        [Fact]
        public async Task GetUserTaskByIdAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.GetUserTaskByIdAsync("task1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUserTaskByIdAsync_ShouldReturnForbid_WhenUserDoesNotOwnTask()
        {
            // Arrange
            SetUser("user1");
            var task = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user2", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(task);

            // Act
            var result = await _controller.GetUserTaskByIdAsync("task1");

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task AddUserTaskAsync_ShouldReturnCreated_WhenTaskIsValid()
        {
            // Arrange
            SetUser("user1");
            var newTask = new TodoTaskCreateRequestDto { Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            var createdTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.AddTaskAsync(newTask)).ReturnsAsync(createdTask);

            // Act
            var result = await _controller.AddUserTaskAsync(newTask);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = createdResult.Value as TodoTaskResponseDto;
            Assert.NotNull(response);
            Assert.Equal(createdTask.Id, response.Id);
            Assert.Equal(createdTask.Title, response.Title);
            Assert.Equal(createdTask.UserId, response.UserId);
        }

        [Fact]
        public async Task UpdateUserTaskAsync_ShouldReturnNoContent_WhenTaskIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var updatedTask = new TodoTaskCreateRequestDto { Title = "Updated Walk the dog", Type = "TaskType" };
            var existingTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.UpdateUserTaskAsync("task1", updatedTask);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            var updatedTask = new TodoTaskCreateRequestDto { Title = "Updated Walk the dog", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.UpdateUserTaskAsync("task1", updatedTask);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUserTaskAsync_ShouldReturnForbid_WhenUserDoesNotOwnTask()
        {
            // Arrange
            SetUser("user1");
            var updatedTask = new TodoTaskCreateRequestDto { Title = "Updated Walk the dog", Type = "TaskType" };
            var existingTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user2", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.UpdateUserTaskAsync("task1", updatedTask);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_ShouldReturnNoContent_WhenTaskIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("user1");
            var existingTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.DeleteUserTaskAsync("task1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            SetUser("user1");
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.DeleteUserTaskAsync("task1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUserTaskAsync_ShouldReturnForbid_WhenUserDoesNotOwnTask()
        {
            // Arrange
            SetUser("user1");
            var existingTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user2", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.DeleteUserTaskAsync("task1");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        // ==================== Admin Tests ====================

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnOk_WhenTasksExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var tasks = new List<TodoTaskResponseDto> { new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" } };
            _mockTodoTaskService.Setup(s => s.GetAllTasksAsync()).ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetAllTasksAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tasks, okResult.Value);
        }

        [Fact]
        public async Task GetTaskByIdForAdminAsync_ShouldReturnOk_WhenTaskExists()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var task = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(task);

            // Act
            var result = await _controller.GetTaskByIdForAdminAsync("task1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(task, okResult.Value);
        }

        [Fact]
        public async Task GetTaskByIdForAdminAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            SetUser("admin1", "Admin");
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.GetTaskByIdForAdminAsync("task1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddTaskForUserAsync_ShouldReturnCreated_WhenTaskIsValid()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var newTask = new TodoTaskCreateRequestDto { Title = "Walk the dog", UserId = "user1", Type = "TaskType" };
            var createdTask = new TodoTaskResponseDto { Id = "task1", Title = "Walk the dog", UserId = "user1" , Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.AddTaskAsync(newTask)).ReturnsAsync(createdTask);

            // Act
            var result = await _controller.AddTaskForUserAsync(newTask);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = createdResult.Value as TodoTaskResponseDto;
            Assert.NotNull(response);
            Assert.Equal(createdTask.Id, response.Id);
            Assert.Equal(createdTask.Title, response.Title);
            Assert.Equal(createdTask.UserId, response.UserId);
        }

        [Fact]
        public async Task UpdateTaskForAdminAsync_ShouldReturnNoContent_WhenTaskIsUpdatedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var updatedTask = new TodoTaskCreateRequestDto { Title = "Updated Walk the dog", DueDate = DateTime.Now.AddDays(2), Type = "TaskType" };
            var existingTask = new TodoTaskResponseDto { Id = "task1", UserId = "user1", Title = "Walk the dog", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.UpdateTaskForAdminAsync("task1", updatedTask);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTaskForAdminAsync_ShouldReturnNoContent_WhenTaskIsDeletedSuccessfully()
        {
            // Arrange
            SetUser("admin1", "Admin");
            var existingTask = new TodoTaskResponseDto { Id = "task1", UserId = "user1", Title = "Walk the dog", Type = "TaskType" };
            _mockTodoTaskService.Setup(s => s.GetTaskByIdAsync("task1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.DeleteTaskForAdminAsync("task1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
