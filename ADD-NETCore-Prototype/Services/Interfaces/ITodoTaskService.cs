using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;

namespace Api.Services.Interfaces
{
    /// <summary>
    /// Interface for Todo Task Service.
    /// </summary>
    public interface ITodoTaskService
    {
        /// <summary>
        /// Gets all tasks asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of TodoTaskResponseDto.</returns>
        Task<IEnumerable<TodoTaskResponseDto>> GetAllTasksAsync();

        /// <summary>
        /// Gets a task by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the task.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TodoTaskResponseDto.</returns>
        Task<TodoTaskResponseDto> GetTaskByIdAsync(string id);

        /// <summary>
        /// Adds a new task asynchronously.
        /// </summary>
        /// <param name="taskDto">The task data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TodoTaskResponseDto.</returns>
        Task<TodoTaskResponseDto> AddTaskAsync(TodoTaskCreateRequestDto taskDto);

        /// <summary>
        /// Updates an existing task asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the task to update.</param>
        /// <param name="updatedTaskDto">The updated task data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TodoTaskResponseDto.</returns>
        Task<TodoTaskResponseDto> UpdateTaskAsync(
            string id,
            TodoTaskCreateRequestDto updatedTaskDto
        );

        /// <summary>
        /// Deletes a task by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the task to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteTaskAsync(string id);
    }
}
