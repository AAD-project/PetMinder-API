namespace Api.Models
{
    public class Pet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Primary Key
        public required string Name { get; set; }
        public required string Gender { get; set; }
        public required string Type { get; set; }

        // Change from DateFormat to DateTime
        public required DateTime DateOfBirth { get; set; }

        public required string Breed { get; set; }
        public float Weight { get; set; }
        public HealthData? HealthData { get; set; }

        public required string OwnerId { get; set; } // Foreign Key for User (Owner)
    }
}
