namespace Api.DTOs
{
    public class PetResponseDto
    {
        public required string Id { get; set; }
        public required string OwnerId { get; set; }
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? Type { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Breed { get; set; }
        public float Weight { get; set; }
        public HealthDataResponseDto? HealthData { get; set; }
    }
}
