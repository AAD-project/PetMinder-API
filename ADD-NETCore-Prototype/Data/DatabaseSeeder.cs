using System.Text.Json;
using Api.Data.Implementations;
using Api.Models;

namespace Api.Data.Seeders
{
    public class DatabaseSeeder(PetMinderDbContext context)
    {
        private readonly PetMinderDbContext _context = context;

        public void Seed()
        {
            // Check if any data exists before seeding
            if (!_context.Users.Any() && !_context.Pets.Any() && !_context.Tasks.Any())
            {
                // Read JSON files (adjust the file paths as needed)
                var usersJson = File.ReadAllText("Data/DummyData/users.json");
                var tasksJson = File.ReadAllText("Data/DummyData/tasks.json");

                // Deserialize JSON data into C# objects
                var users = JsonSerializer.Deserialize<List<User>>(usersJson);
                var tasks = JsonSerializer.Deserialize<List<TodoTask>>(tasksJson);

                // Add data to the DbContext
#pragma warning disable CS8604 // Possible null reference argument.
                _context.Users.AddRange(users);
                _context.Tasks.AddRange(tasks);
#pragma warning restore CS8604 // Possible null reference argument.

                // Save changes to the database
                _context.SaveChanges();
            }
        }
    }
}
