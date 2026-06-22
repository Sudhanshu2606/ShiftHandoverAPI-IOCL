namespace ShiftHandoverAPI.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Supervisor";
        public string Department { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string ShiftAssigned { get; set; } = "Morning";
        public string Batch { get; set; } = "General";
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}