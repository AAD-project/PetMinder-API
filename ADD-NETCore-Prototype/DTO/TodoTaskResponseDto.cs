namespace Api.DTOs
{
    public class TodoTaskResponseDto
    {
        public required string Id { get; set; }
        public required string Type { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}
