namespace Api.DTOs
{
    public class UserCreateRequestDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        // Password is required for creating a user in ASP.NET Core Identity
        public required string Password { get; set; }

        // Optional: Other user-related properties can go here.
    }
}
