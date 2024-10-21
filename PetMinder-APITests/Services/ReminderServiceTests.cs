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
    public class ReminderServiceTests
    {
        private readonly Mock<IDbContextFactory> _mockDbContextFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ReminderService _reminderService;
        private readonly DbContextOptions<PetMinderDbContext> _options;

        public ReminderServiceTests()
        {
            // Set up in-memory database
            _options = new DbContextOptionsBuilder<PetMinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique DB for each test
                .Options;

            _mockDbContextFactory = new Mock<IDbContextFactory>();
            _mockDbContextFactory.Setup(f => f.CreateDbContext()).Returns(() => new PetMinderDbContext(_options));
            _mockMapper = new Mock<IMapper>();

            _reminderService = new ReminderService(_mockDbContextFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateReminderAsync_ShouldCreateReminder_WhenDataIsValid()
        {
            // Arrange
            var reminderDto = new ReminderCreateRequestDto { Title = "Vet Appointment", UserId = "user1", PetId = "pet1" };
            var reminder = new Reminder
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Vet Appointment",
                UserId = "user1",
                ReminderDateTime = DateTime.UtcNow.AddDays(1)
            };
            var reminderResponseDto = new ReminderResponseDto { Id = reminder.Id, Title = "Vet Appointment", UserId = "user1" };

            _mockMapper.Setup(m => m.Map<Reminder>(reminderDto)).Returns(reminder);
            _mockMapper.Setup(m => m.Map<ReminderResponseDto>(reminder)).Returns(reminderResponseDto);

            // Act
            var result = await _reminderService.CreateReminderAsync(reminderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reminderResponseDto.Id, result.Id);
            Assert.Equal(reminderResponseDto.Title, result.Title);
            Assert.Equal(reminderResponseDto.UserId, result.UserId);
        }

        [Fact]
        public async Task CreateReminderAsync_ShouldThrowArgumentNullException_WhenReminderDtoIsNull()
        {
            // Arrange
            ReminderCreateRequestDto reminderDto = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _reminderService.CreateReminderAsync(reminderDto));
        }

        [Fact]
        public async Task GetReminderByIdAsync_ShouldReturnReminder_WhenReminderExists()
        {
            // Arrange
            var reminderId = "reminder1";
            var reminder = new Reminder
            {
                Id = reminderId,
                Title = "Vet Appointment",
                UserId = "user1",
                ReminderDateTime = DateTime.UtcNow.AddDays(1)
            };
            var reminderDto = new ReminderResponseDto { Id = reminderId, Title = "Vet Appointment", UserId = "user1" };

            using (var context = new PetMinderDbContext(_options))
            {
                context.Reminders.Add(reminder);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<ReminderResponseDto>(It.IsAny<Reminder>())).Returns(reminderDto);

            // Act
            var result = await _reminderService.GetReminderByIdAsync(reminderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reminderDto.Id, result.Id);
            Assert.Equal(reminderDto.Title, result.Title);
            Assert.Equal(reminderDto.UserId, result.UserId);
        }

        [Fact]
        public async Task GetReminderByIdAsync_ShouldThrowKeyNotFoundException_WhenReminderDoesNotExist()
        {
            // Arrange
            var reminderId = "non_existing_reminder";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reminderService.GetReminderByIdAsync(reminderId));
        }

        [Fact]
        public async Task GetAllRemindersByUserIdAsync_ShouldReturnReminders_WhenRemindersExistForUser()
        {
            // Arrange
            var userId = "user1";
            var reminders = new List<Reminder>
            {
                new Reminder
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Vet Appointment",
                    ReminderDateTime = DateTime.UtcNow.AddDays(1)
                }
            };
            var reminderDtos = reminders.Select(r => new ReminderResponseDto { Id = r.Id, UserId = userId, Title = r.Title }).ToList();

            using (var context = new PetMinderDbContext(_options))
            {
                context.Reminders.AddRange(reminders);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<IEnumerable<ReminderResponseDto>>(It.IsAny<List<Reminder>>())).Returns(reminderDtos);

            // Act
            var result = await _reminderService.GetAllRemindersByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reminderDtos.Count, result.Count());
            Assert.Equal(reminderDtos.Select(r => r.Id), result.Select(r => r.Id));
        }

        [Fact]
        public async Task UpdateReminderAsync_ShouldThrowKeyNotFoundException_WhenReminderDoesNotExist()
        {
            // Arrange
            var reminderId = "non_existing_reminder";
            var updatedReminderDto = new ReminderCreateRequestDto { Title = "Updated Vet Appointment", UserId = "user1", PetId = "pet1" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reminderService.UpdateReminderAsync(reminderId, updatedReminderDto));
        }

        [Fact]
        public async Task DeleteReminderAsync_ShouldRemoveReminder_WhenReminderExists()
        {
            // Arrange
            var reminderId = Guid.NewGuid().ToString();
            var reminder = new Reminder
            {
                Id = reminderId,
                Title = "Vet Appointment",
                UserId = "user1",
                ReminderDateTime = DateTime.UtcNow.AddDays(1)
            };

            using (var context = new PetMinderDbContext(_options))
            {
                context.Reminders.Add(reminder);
                await context.SaveChangesAsync();
            }

            // Act
            await _reminderService.DeleteReminderAsync(reminderId);

            // Assert
            using (var context = new PetMinderDbContext(_options))
            {
                var deletedReminder = await context.Reminders.FirstOrDefaultAsync(r => r.Id == reminderId);
                Assert.Null(deletedReminder);
            }
        }

        [Fact]
        public async Task DeleteReminderAsync_ShouldThrowKeyNotFoundException_WhenReminderDoesNotExist()
        {
            // Arrange
            var reminderId = "non_existing_reminder";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reminderService.DeleteReminderAsync(reminderId));
        }

        [Fact]
        public async Task GetAllRemindersByUserIdAsync_ShouldReturnEmptyList_WhenNoRemindersExistForUser()
        {
            // Arrange
            var userId = "user1";

            using (var context = new PetMinderDbContext(_options))
            {
                context.Reminders.RemoveRange(context.Reminders);
                await context.SaveChangesAsync();
            }

            _mockMapper.Setup(m => m.Map<IEnumerable<ReminderResponseDto>>(It.IsAny<List<Reminder>>())).Returns(new List<ReminderResponseDto>());

            // Act
            var result = await _reminderService.GetAllRemindersByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
