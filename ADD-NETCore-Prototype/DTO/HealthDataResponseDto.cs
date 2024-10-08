namespace Api.DTOs
{
    public class HealthDataResponseDto
    {
        public required string Id { get; set; }
        public required string PetId { get; set; }
        public List<float>? WeightHistory { get; set; }
        public DateTime? LastVetVisit { get; set; }
        public List<string>? Vaccinations { get; set; }
        public List<string>? Allergies { get; set; }
        public string? MedicalNotes { get; set; }
        public List<string>? CurrentMedications { get; set; }
        public float CurrentWeight { get; set; }
    }
}
