using System.Text.Json.Serialization;

namespace ShiftHandoverAPI.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public int HandoverId { get; set; }
        [JsonIgnore]
        public HandoverReport? Handover { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public string Status { get; set; } = "Open";
    }
}