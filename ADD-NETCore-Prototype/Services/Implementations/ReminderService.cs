using Api.Data.Interface;
using Api.DTOs;
using Api.Models;
using Api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Implementations
{
    public class ReminderService : BaseService, IReminderService
    {
        private readonly IMapper _mapper;

        public ReminderService(IDbContextFactory dbContextFactory, IMapper mapper)
            : base(dbContextFactory)
        {
            _mapper = mapper;
        }

        // Create a new reminder
        public async Task<ReminderResponseDto> CreateReminderAsync(
            ReminderCreateRequestDto reminderDto
        )
        {
            if (reminderDto == null)
                throw new ArgumentNullException(nameof(reminderDto), "Reminder cannot be null.");

            using var dbContext = await CreateDbContextAsync();

            // Map the ReminderCreateRequestDto to the Reminder entity
            var reminder = _mapper.Map<Reminder>(reminderDto);
            dbContext.Reminders.Add(reminder);
            await dbContext.SaveChangesAsync();

            // Return the newly created reminder as a response DTO
            return _mapper.Map<ReminderResponseDto>(reminder);
        }

        // Get a reminder by its ID
        public async Task<ReminderResponseDto> GetReminderByIdAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var reminder =
                await dbContext.Reminders.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Reminder with Id {id} not found.");

            // Map the Reminder entity to ReminderResponseDto
            return _mapper.Map<ReminderResponseDto>(reminder);
        }

        // Get all reminders for a specific user
        public async Task<IEnumerable<ReminderResponseDto>> GetAllRemindersByUserIdAsync(
            string userId
        )
        {
            using var dbContext = await CreateDbContextAsync();
            var reminders = await dbContext.Reminders.Where(r => r.UserId == userId).ToListAsync();

            // Map list of Reminder entities to list of ReminderResponseDto
            return _mapper.Map<IEnumerable<ReminderResponseDto>>(reminders);
        }

        // Update an existing reminder
        public async Task<ReminderResponseDto> UpdateReminderAsync(
            string id,
            ReminderCreateRequestDto updatedReminderDto
        )
        {
            if (updatedReminderDto == null)
                throw new ArgumentNullException(
                    nameof(updatedReminderDto),
                    "Updated reminder cannot be null."
                );

            using var dbContext = await CreateDbContextAsync();
            var existingReminder =
                await dbContext.Reminders.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Reminder with Id {id} not found.");

            // Map the updated values from the DTO to the existing entity
            _mapper.Map(updatedReminderDto, existingReminder);
            await dbContext.SaveChangesAsync();

            // Return the updated reminder as a response DTO
            return _mapper.Map<ReminderResponseDto>(existingReminder);
        }

        // Delete a reminder by its ID
        public async Task DeleteReminderAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var reminder =
                await dbContext.Reminders.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Reminder with Id {id} not found.");

            dbContext.Reminders.Remove(reminder);
            await dbContext.SaveChangesAsync();
        }
    }
}
