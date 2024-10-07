namespace Api.DTOs
{
    public class ReminderResponseDto
    {
        public required string Id { get; set; }
        public required string UserId { get; set; }
        public string? PetId { get; set; }
        public required string Title { get; set; }
        public string? Message { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public List<DateTime>? NextReminderDateTimeList { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDue { get; set; }
    }
}
