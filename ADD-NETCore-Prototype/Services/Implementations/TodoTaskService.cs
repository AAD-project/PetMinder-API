using Api.Data.Interface;
using Api.DTOs;
using Api.Models;
using Api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Implementations
{
    public class TodoTaskService : BaseService, ITodoTaskService
    {
        private readonly IMapper _mapper;

        public TodoTaskService(IDbContextFactory dbContextFactory, IMapper mapper)
            : base(dbContextFactory)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<TodoTaskResponseDto>> GetAllTasksAsync()
        {
            using var dbContext = await CreateDbContextAsync();
            var tasks = await dbContext.Tasks.ToListAsync();
            return _mapper.Map<IEnumerable<TodoTaskResponseDto>>(tasks);
        }

        public async Task<TodoTaskResponseDto> GetTaskByIdAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var task =
                await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Task with Id {id} not found.");
            return _mapper.Map<TodoTaskResponseDto>(task);
        }

        public async Task<TodoTaskResponseDto> AddTaskAsync(TodoTaskCreateRequestDto taskDto)
        {
            if (taskDto == null)
                throw new ArgumentNullException(nameof(taskDto), "Task cannot be null.");

            using var dbContext = await CreateDbContextAsync();
            var task = _mapper.Map<TodoTask>(taskDto);
            await dbContext.Tasks.AddAsync(task);
            await dbContext.SaveChangesAsync();
            return _mapper.Map<TodoTaskResponseDto>(task);
        }

        public async Task<TodoTaskResponseDto> UpdateTaskAsync(
            string id,
            TodoTaskCreateRequestDto updatedTaskDto
        )
        {
            if (updatedTaskDto == null)
                throw new ArgumentNullException(nameof(updatedTaskDto), "Task cannot be null.");

            using var dbContext = await CreateDbContextAsync();
            var existingTask =
                await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Task with Id {id} not found.");

            _mapper.Map(updatedTaskDto, existingTask);
            await dbContext.SaveChangesAsync();
            return _mapper.Map<TodoTaskResponseDto>(existingTask);
        }

        public async Task DeleteTaskAsync(string id)
        {
            using var dbContext = await CreateDbContextAsync();
            var task =
                await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Task with Id {id} not found.");

            dbContext.Tasks.Remove(task);
            await dbContext.SaveChangesAsync();
        }
    }
}
