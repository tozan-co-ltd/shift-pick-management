using System;

namespace shift_pick_management.Models
{
    public class M_PrefectureModel
    {
        public int PrefectureID { get; set; }
        public string PrefectureName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool Registered { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
