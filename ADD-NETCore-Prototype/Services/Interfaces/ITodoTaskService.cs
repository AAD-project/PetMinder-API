using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;

namespace Api.Services.Interfaces
{
    public interface ITodoTaskService
    {
        Task<IEnumerable<TodoTaskResponseDto>> GetAllTasksAsync();
        Task<TodoTaskResponseDto> GetTaskByIdAsync(string id);
        Task<TodoTaskResponseDto> AddTaskAsync(TodoTaskCreateRequestDto taskDto);
        Task<TodoTaskResponseDto> UpdateTaskAsync(
            string id,
            TodoTaskCreateRequestDto updatedTaskDto
        );
        Task DeleteTaskAsync(string id);
    }
}
