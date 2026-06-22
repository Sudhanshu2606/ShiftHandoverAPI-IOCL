using System.Text.Json.Serialization;

namespace ShiftHandoverAPI.Models
{
    public class HandoverReport
    {
        public int Id { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }

        [JsonIgnore]
        public User? Creator { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";

        // ⭐ UNIT FIELDS - IMPORTANT FOR UNIT DISPLAY ⭐
        public string? Unit { get; set; }
        public string? UnitName { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();
        public List<Issue> Issues { get; set; } = new();
    }
}