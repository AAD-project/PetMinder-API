using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;

namespace Api.Services.Interfaces
{
    public interface IReminderService
    {
        Task<ReminderResponseDto> CreateReminderAsync(ReminderCreateRequestDto reminderDto);
        Task<ReminderResponseDto> GetReminderByIdAsync(string id);
        Task<IEnumerable<ReminderResponseDto>> GetAllRemindersByUserIdAsync(string userId);
        Task<ReminderResponseDto> UpdateReminderAsync(
            string id,
            ReminderCreateRequestDto updatedReminderDto
        );
        Task DeleteReminderAsync(string id);
    }
}
