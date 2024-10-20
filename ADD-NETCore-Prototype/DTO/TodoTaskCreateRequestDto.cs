namespace Api.DTOs
{
    public class TodoTaskCreateRequestDto
    {
        public required string Type { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }
        public string? UserId { get; set; }
        public string? PetId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
