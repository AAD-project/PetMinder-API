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
    public class TodoTaskController : ControllerBase
    {
        private readonly ITodoTaskService _todoTaskService;

        public TodoTaskController(ITodoTaskService todoTaskService)
        {
            _todoTaskService = todoTaskService;
        }

        // Helper method to get the authenticated user's ID
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ==================== Regular User Endpoints ====================

        // GET: api/todotask/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<TodoTaskResponseDto>>> GetUserTasksAsync()
        {
            try
            {
                var userId = GetUserId();
                var tasks = await _todoTaskService.GetUserTasksAsync(userId);
                return Ok(tasks);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // GET: api/todotask/user/{id}
        [HttpGet("user/{id}")]
        public async Task<ActionResult<TodoTaskResponseDto>> GetUserTaskByIdAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Ensure only the task owner can access the task
                if (task.UserId == GetUserId())
                {
                    return Ok(task);
                }
                else
                {
                    return Forbid("You do not have permission to access this task.");
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

        // POST: api/todotask/user
        [HttpPost("user")]
        public async Task<ActionResult<TodoTaskResponseDto>> AddUserTaskAsync(
            [FromBody] TodoTaskCreateRequestDto newTaskDto
        )
        {
            if (newTaskDto == null)
            {
                return BadRequest("Task data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Assign the task to the current authenticated user
                newTaskDto.UserId = GetUserId();

                var addedTask = await _todoTaskService.AddTaskAsync(newTaskDto);
                return CreatedAtAction(
                    nameof(GetUserTaskByIdAsync),
                    new { id = addedTask.Id },
                    addedTask
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

        // PATCH: api/todotask/user/{id}
        [HttpPatch("user/{id}")]
        public async Task<IActionResult> UpdateUserTaskAsync(
            string id,
            [FromBody] TodoTaskCreateRequestDto updatedTaskDto
        )
        {
            if (updatedTaskDto == null)
            {
                return BadRequest("Task data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingTask = await _todoTaskService.GetTaskByIdAsync(id);

                if (existingTask == null)
                {
                    return NotFound();
                }

                // Ensure only the owner of the task can update it
                if (existingTask.UserId == GetUserId())
                {
                    await _todoTaskService.UpdateTaskAsync(id, updatedTaskDto);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to update this task.");
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

        // DELETE: api/todotask/user/{id}
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUserTaskAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Ensure only the owner of the task can delete it
                if (task.UserId == GetUserId())
                {
                    await _todoTaskService.DeleteTaskAsync(id);
                    return NoContent();
                }
                else
                {
                    return Forbid("You do not have permission to delete this task.");
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

        // GET: api/todotask/admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<TodoTaskResponseDto>>> GetAllTasksAsync()
        {
            try
            {
                var tasks = await _todoTaskService.GetAllTasksAsync();
                return Ok(tasks);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // GET: api/todotask/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<TodoTaskResponseDto>> GetTaskByIdForAdminAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Admins can access any task
                return Ok(task);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while processing your request: {ex.Message}"
                );
            }
        }

        // POST: api/todotask/admin
        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<ActionResult<TodoTaskResponseDto>> AddTaskForUserAsync(
            [FromBody] TodoTaskCreateRequestDto newTaskDto
        )
        {
            if (newTaskDto == null)
            {
                return BadRequest("Task data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Admin must assign the UserId for the task
                if (string.IsNullOrEmpty(newTaskDto.UserId))
                {
                    return BadRequest(
                        "UserId must be provided to create a task for a specific user."
                    );
                }

                var addedTask = await _todoTaskService.AddTaskAsync(newTaskDto);
                return CreatedAtAction(
                    nameof(GetTaskByIdForAdminAsync),
                    new { id = addedTask.Id },
                    addedTask
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

        // PATCH: api/todotask/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/{id}")]
        public async Task<IActionResult> UpdateTaskForAdminAsync(
            string id,
            [FromBody] TodoTaskCreateRequestDto updatedTaskDto
        )
        {
            if (updatedTaskDto == null)
            {
                return BadRequest("Task data is invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingTask = await _todoTaskService.GetTaskByIdAsync(id);

                if (existingTask == null)
                {
                    return NotFound();
                }

                // Admins can update any task
                await _todoTaskService.UpdateTaskAsync(id, updatedTaskDto);
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

        // DELETE: api/todotask/admin/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteTaskForAdminAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Admins can delete any task
                await _todoTaskService.DeleteTaskAsync(id);
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
