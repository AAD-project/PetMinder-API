namespace Api.DTOs
{
    public class PetCreateRequestDto
    {
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? Type { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Breed { get; set; }
        public float Weight { get; set; }
        public string? OwnerId { get; set; }
    }
}
