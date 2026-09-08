using System;

namespace shift_pick_management.Models
{
    public class OuboConfirmationModel
    {
        public string UserName { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
