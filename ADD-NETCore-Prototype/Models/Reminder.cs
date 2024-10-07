namespace Api.Models
{
    public class Reminder
    {
        public required string Id { get; set; }
        public required string UserId { get; set; } // Foreign Key to the user who set the reminder
        public string? PetId { get; set; } // Foreign Key to the associated pet
        public required string Title { get; set; } // Title of the reminder
        public string? Message { get; set; } // Message that will be shown in the notification
        public DateTime ReminderDateTime { get; set; } // The date and time for the reminder
        public bool IsRecurring { get; set; } // If the reminder is recurring
        public string? RecurrencePattern { get; set; } // Could be 'Daily', 'Weekly', etc., or use a more specific pattern like cron expressions
        public List<DateTime>? NextReminderDateTimeList { get; set; } // If recurring, the next occurrence
        public bool IsCompleted { get; set; } // Track whether the reminder has been completed

        // Helper method to check if a reminder is due
        public bool IsDue
        {
            get { return ReminderDateTime <= DateTime.Now && !IsCompleted; }
        }
    }
}
