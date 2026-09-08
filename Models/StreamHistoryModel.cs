using System;

namespace shift_pick_management.Models
{
    public class StreamHistoryModel
    {
        public int HistoryID { get; set; }
        public string StreamName { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool Acknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
