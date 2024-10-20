namespace Api.Models
{
    public class TodoTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Type { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }

        // Foreign key to User, no navigation property
        public required string UserId { get; set; } // Foreign Key to User

        public string? PetId { get; set; }
    }
}
