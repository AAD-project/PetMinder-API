using System.Security.Claims;
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

        // Helper method to get the authenticated user's ID
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ==================== Regular User Endpoints ====================

        // POST: api/reminder/user
        [HttpPost("user")]
        public async Task<ActionResult<ReminderResponseDto>> CreateUserReminderAsync(
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
                // Set the owner of the reminder to the current authenticated user
                reminderDto.UserId = GetUserId();

                var createdReminder = await _reminderService.CreateReminderAsync(reminderDto);
                return CreatedAtAction(
                    nameof(GetUserReminderByIdAsync),
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

        // GET: api/reminder/user/{id}
        [HttpGet("user/{id}")]
        public async Task<ActionResult<ReminderResponseDto>> GetUserReminderByIdAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                // Only the owner of the reminder can access it
                if (reminder.UserId == GetUserId())
                {
                    return Ok(reminder);
                }
                else
                {
                    return Forbid("You do not have permission to access this reminder.");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // GET: api/reminder/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetAllUserRemindersAsync()
        {
            try
            {
                var userId = GetUserId();
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

        // PATCH: api/reminder/user/{id}
        [HttpPatch("user/{id}")]
        public async Task<IActionResult> UpdateUserReminderAsync(
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

                // Only the owner of the reminder can update it
                if (existingReminder.UserId == GetUserId())
                {
                    await _reminderService.UpdateReminderAsync(id, reminderDto);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to update this reminder.");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // DELETE: api/reminder/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUserReminderAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                // Only the owner of the reminder can delete it
                if (reminder.UserId == GetUserId())
                {
                    await _reminderService.DeleteReminderAsync(id);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to delete this reminder.");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // ==================== Admin Endpoints ====================

        // GET: api/reminder/admin/user/{userId}
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/user/{userId}")]
        public async Task<
            ActionResult<IEnumerable<ReminderResponseDto>>
        > GetAllRemindersByUserIdForAdminAsync(string userId)
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

        // GET: api/reminder/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<ReminderResponseDto>> GetReminderByIdForAdminAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                // Admins can access any reminder
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

        // POST: api/reminder/admin
        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<ActionResult<ReminderResponseDto>> CreateReminderForUserAsync(
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
                if (string.IsNullOrEmpty(reminderDto.UserId))
                {
                    return BadRequest(
                        "UserId must be provided when creating a reminder for another user."
                    );
                }

                var createdReminder = await _reminderService.CreateReminderAsync(reminderDto);
                return CreatedAtAction(
                    nameof(GetReminderByIdForAdminAsync),
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

        // PATCH: api/reminder/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/{id}")]
        public async Task<IActionResult> UpdateReminderForAdminAsync(
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

                // Admins can update any reminder
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

        // DELETE: api/reminder/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteReminderForAdminAsync(string id)
        {
            try
            {
                var reminder = await _reminderService.GetReminderByIdAsync(id);

                if (reminder == null)
                {
                    return NotFound();
                }

                // Admins can delete any reminder
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
