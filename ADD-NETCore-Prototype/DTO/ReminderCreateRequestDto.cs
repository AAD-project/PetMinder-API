namespace Api.DTOs
{
    public class ReminderCreateRequestDto
    {
        public required string UserId { get; set; }
        public required string? PetId { get; set; }
        public required string Title { get; set; }
        public string? Message { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
    }
}
