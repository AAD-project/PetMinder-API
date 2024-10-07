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

        [HttpGet]
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

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoTaskResponseDto>> GetTaskByIdAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

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

        [HttpPost]
        public async Task<ActionResult<TodoTaskResponseDto>> AddTaskAsync(
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
                var addedTask = await _todoTaskService.AddTaskAsync(newTaskDto);
                return CreatedAtAction(
                    nameof(GetTaskByIdAsync),
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTaskAsync(
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskAsync(string id)
        {
            try
            {
                var task = await _todoTaskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

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
