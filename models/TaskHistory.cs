using System.Text.Json.Serialization;

namespace ShiftHandoverAPI.Models
{
    // Stores an audit trail entry every time a task is edited (status changed,
    // reassigned, or any field updated) or deleted. We deliberately do NOT
    // keep an EF navigation/foreign-key relationship to TaskItem, because the
    // task row may later be deleted (DELETE /api/Task/{id}) and we still want
    // the history record to remain — so we just snapshot what we need.
    public class TaskHistory
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public int HandoverId { get; set; }

        // "Created", "StatusChanged", "Reassigned", "Updated", "Deleted"
        public string Action { get; set; } = string.Empty;

        // Human-readable description of what changed, e.g.
        // "Status changed from Pending to InProgress"
        public string Details { get; set; } = string.Empty;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int ChangedBy { get; set; }
        public string ChangedByName { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}