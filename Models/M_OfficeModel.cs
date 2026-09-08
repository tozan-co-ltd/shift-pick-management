using System;

namespace shift_pick_management.Models
{
    public class M_OfficeModel
    {
        public int OfficeID { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool Registered { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
