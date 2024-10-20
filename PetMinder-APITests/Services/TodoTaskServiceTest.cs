using System.Collections.Generic;
using System.Threading.Tasks;
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
using System.Linq;

namespace Api.Tests.Services
{
    public class TodoTaskServiceTests
    {
        private readonly Mock<IDbContextFactory> _mockDbContextFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TodoTaskService _todoTaskService;
        private readonly DbContextOptions<PetMinderDbContext> _options;

        public TodoTaskServiceTests()
        {
            // Set up in-memory database
            _options = new DbContextOptionsBuilder<PetMinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _mockDbContextFactory = new Mock<IDbContextFactory>();
            _mockDbContextFactory.Setup(f => f.CreateDbContext()).Returns(() => new PetMinderDbContext(_options));
            _mockMapper = new Mock<IMapper>();

            _todoTaskService = new TodoTaskService(_mockDbContextFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks_WhenTasksExist()
        {
            // Arrange
            var tasks = new List<TodoTask>
            {
                new TodoTask { Id = "task1", Title = "Walk the dog", UserId = "user1", Type = "TaskType1" },
                new TodoTask { Id = "task2", Title = "Feed the cat", UserId = "user2", Type = "TaskType2" }
            };
            var taskDtos = tasks.Select(t => new TodoTaskResponseDto { Id = t.Id, Title = t.Title, UserId = t.UserId, Type = t.Type }).ToList();

            using (var context = new PetMinderDbContext(_options))
            {
                context.Tasks.AddRange(tasks);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<IEnumerable<TodoTaskResponseDto>>(It.IsAny<List<TodoTask>>())).Returns(taskDtos);

            // Act
            var result = await _todoTaskService.GetAllTasksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskDtos.Count, result.Count());
            Assert.Equal(taskDtos.Select(t => t.Id), result.Select(t => t.Id));
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var taskId = "task1";
            var task = new TodoTask { Id = taskId, Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };
            var taskDto = new TodoTaskResponseDto { Id = taskId, Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };

            using (var context = new PetMinderDbContext(_options))
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<TodoTaskResponseDto>(It.IsAny<TodoTask>())).Returns(taskDto);

            // Act
            var result = await _todoTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskDto.Id, result.Id);
            Assert.Equal(taskDto.Title, result.Title);
            Assert.Equal(taskDto.UserId, result.UserId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldThrowKeyNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = "non_existing_task";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _todoTaskService.GetTaskByIdAsync(taskId));
        }

        [Fact]
        public async Task AddTaskAsync_ShouldAddTask_WhenDataIsValid()
        {
            // Arrange
            var taskDto = new TodoTaskCreateRequestDto { Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };
            var task = new TodoTask { Id = Guid.NewGuid().ToString(), Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };
            var taskResponseDto = new TodoTaskResponseDto { Id = task.Id, Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };

            _mockMapper.Setup(m => m.Map<TodoTask>(taskDto)).Returns(task);
            _mockMapper.Setup(m => m.Map<TodoTaskResponseDto>(task)).Returns(taskResponseDto);

            // Act
            var result = await _todoTaskService.AddTaskAsync(taskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskResponseDto.Id, result.Id);
            Assert.Equal(taskResponseDto.Title, result.Title);
            Assert.Equal(taskResponseDto.UserId, result.UserId);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldRemoveTask_WhenTaskExists()
        {
            // Arrange
            var taskId = Guid.NewGuid().ToString();
            var task = new TodoTask { Id = taskId, Title = "Walk the dog", UserId = "user1", Type = "TaskType1" };

            using (var context = new PetMinderDbContext(_options))
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Act
            await _todoTaskService.DeleteTaskAsync(taskId);

            // Assert
            using (var context = new PetMinderDbContext(_options))
            {
                var deletedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                Assert.Null(deletedTask);
            }
        }

        [Fact]
        public async Task GetUserTasksAsync_ShouldReturnTasks_WhenTasksExistForUser()
        {
            // Arrange
            var userId = "user1";
            var tasks = new List<TodoTask>
            {
                new TodoTask { Id = Guid.NewGuid().ToString(), Title = "Walk the dog", UserId = userId, Type = "TaskType1" },
                new TodoTask { Id = Guid.NewGuid().ToString(), Title = "Feed the cat", UserId = userId, Type = "TaskType2" }
            };
            var taskDtos = tasks.Select(t => new TodoTaskResponseDto { Id = t.Id, Title = t.Title, UserId = t.UserId, Type = t.Type }).ToList();

            using (var context = new PetMinderDbContext(_options))
            {
                context.Tasks.AddRange(tasks);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<IEnumerable<TodoTaskResponseDto>>(It.IsAny<List<TodoTask>>())).Returns(taskDtos);

            // Act
            var result = await _todoTaskService.GetUserTasksAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskDtos.Count, result.Count());
            Assert.Equal(taskDtos.Select(t => t.Id), result.Select(t => t.Id));
        }
    }
}
