using System;

namespace shift_pick_management.Models
{
    public class NotificationStreamModel
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
