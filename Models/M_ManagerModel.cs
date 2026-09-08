using System;

namespace shift_pick_management.Models
{
    public class M_ManagerModel
    {
        public int ManagerID { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool Registered { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
