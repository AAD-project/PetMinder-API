using Microsoft.VisualBasic;

namespace Api.Models
{
    public class Pet
    {
        public string Id { get; set; } // Primary Key
        public required string Name { get; set; }
        public required string Gender { get; set; }
        public required string Type { get; set; }
        public required DateFormat DateOfBirth { get; set; }
        public required string Breed { get; set; }
        public float Weight { get; set; }
        public HealthData? HealthData { get; set; }

        // Foreign key for User (Owner), no navigation property
        public required string OwnerId { get; set; } // Nullable Foreign Key
    }
}
