namespace Api.DTOs
{
    public class UserResponseDto
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public List<PetResponseDto>? Pets { get; set; }
        public List<TodoTaskResponseDto>? Tasks { get; set; }
        public List<ReminderResponseDto>? Reminders { get; set; }
    }
}
