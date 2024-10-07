using Microsoft.AspNetCore.Identity;

namespace Api.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<Pet>? Pets { get; set; }
        public List<TodoTask>? Tasks { get; set; }
        public List<Reminder>? Reminders { get; set; }
    }
}
