using Api.Controllers;
using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetMinder_APITests.Controllers
{
    public class TodoTaskControllerTest
    {
        private readonly Mock<ITodoTaskService> _mockTodoTaskService;
        private readonly TodoTaskController _controller;

        public TodoTaskControllerTest()
        {
            _mockTodoTaskService = new Mock<ITodoTaskService>();
            _controller = new TodoTaskController(_mockTodoTaskService.Object);
        }

        [Fact]
        public async Task GetAllTasksAsync_ReturnsOkResult_WithListOfTasks()
        {
            // Arrange
            var tasks = new List<TodoTaskResponseDto>
            {
                new TodoTaskResponseDto { Id = "1", Title = "Task 1", Type = "DefaultType" },
                new TodoTaskResponseDto { Id = "2", Title = "Task 2", Type = "SomeType" }
            };
            _mockTodoTaskService.Setup(service => service.GetAllTasksAsync()).ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetAllTasksAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnTasks = Assert.IsType<List<TodoTaskResponseDto>>(okResult.Value);
            Assert.Equal(2, returnTasks.Count);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync(It.IsAny<string>())).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.GetTaskByIdAsync("1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsOkResult_WithTask()
        {
            // Arrange
            var task = new TodoTaskResponseDto { Id = "1", Title = "Task 1", Type = "DefaultType" };
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync("1")).ReturnsAsync(task);

            // Act
            var result = await _controller.GetTaskByIdAsync("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnTask = Assert.IsType<TodoTaskResponseDto>(okResult.Value);
            Assert.Equal("1", returnTask.Id);
        }

        [Fact]
        public async Task AddTaskAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.AddTaskAsync(new TodoTaskCreateRequestDto { Title = "Sample Title", Type = "Sample Type" });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddTaskAsync_ReturnsCreatedAtAction_WithNewTask()
        {
            // Arrange
            var newTaskDto = new TodoTaskCreateRequestDto { Title = "New Task", Type = "DefaultType" };
            var addedTask = new TodoTaskResponseDto { Id = "1", Title = "New Task", Type = "DefaultType" };
            _mockTodoTaskService.Setup(service => service.AddTaskAsync(newTaskDto)).ReturnsAsync(addedTask);

            // Act
            var result = await _controller.AddTaskAsync(newTaskDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnTask = Assert.IsType<TodoTaskResponseDto>(createdAtActionResult.Value);
            Assert.Equal("1", returnTask.Id);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync(It.IsAny<string>())).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.UpdateTaskAsync("1", new TodoTaskCreateRequestDto { Title = "Updated Task", Type = "DefaultType" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsNoContent_WhenTaskIsUpdated()
        {
            // Arrange
            var existingTask = new TodoTaskResponseDto { Id = "1", Title = "Existing Task", Type = "DefaultType" };
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync("1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.UpdateTaskAsync("1", new TodoTaskCreateRequestDto { Title = "Updated Task", Type = "DefaultType" });

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync(It.IsAny<string>())).ReturnsAsync((TodoTaskResponseDto)null);

            // Act
            var result = await _controller.DeleteTaskAsync("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsNoContent_WhenTaskIsDeleted()
        {
            // Arrange
            var existingTask = new TodoTaskResponseDto { Id = "1", Title = "Existing Task", Type = "DefaultType" };
            _mockTodoTaskService.Setup(service => service.GetTaskByIdAsync("1")).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.DeleteTaskAsync("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetAllTasksAsync_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            _mockTodoTaskService.Setup(service => service.GetAllTasksAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllTasksAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}