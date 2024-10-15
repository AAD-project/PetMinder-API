namespace Api.DTOs
{
    public class HealthDataCreateRequestDto
    {
        public List<float>? WeightHistory { get; set; }
        public DateTime? LastVetVisit { get; set; }
        public List<string>? Vaccinations { get; set; }
        public List<string>? Allergies { get; set; }
        public string? MedicalNotes { get; set; }
        public List<string>? CurrentMedications { get; set; }
    }
}
