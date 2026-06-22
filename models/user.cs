namespace ShiftHandoverAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Supervisor";
        public string Department { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string ShiftAssigned { get; set; } = "Morning";
        public string Batch { get; set; } = "General";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }  // Added for tracking last login
    }
}