using Api.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpPost]
        public async Task<ActionResult<ReminderResponseDto>> CreateReminderAsync(
            [FromBody] ReminderCreateRequestDto reminderDto
        )
        {
            if (reminderDto == null)
            {
                return BadRequest("Reminder data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdReminder = await _reminderService.CreateReminderAsync(reminderDto);
                return CreatedAtAction(
                    nameof(CreateReminderAsync),
                    new { id = createdReminder.Id },
                    createdReminder
                );
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReminderResponseDto>> GetReminderByIdAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                return Ok(reminder);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<
            ActionResult<IEnumerable<ReminderResponseDto>>
        > GetAllRemindersByUserIdAsync(string userId)
        {
            try
            {
                var reminders = await _reminderService.GetAllRemindersByUserIdAsync(userId);
                return Ok(reminders);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReminderAsync(
            string id,
            [FromBody] ReminderCreateRequestDto reminderDto
        )
        {
            if (reminderDto == null)
            {
                return BadRequest("Reminder data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingReminder = await _reminderService.GetReminderByIdAsync(id);

                if (existingReminder == null)
                {
                    return NotFound();
                }

                await _reminderService.UpdateReminderAsync(id, reminderDto);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminderAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                await _reminderService.DeleteReminderAsync(id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }
    }
}
