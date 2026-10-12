using System.ComponentModel.DataAnnotations;

namespace shift_pick_management.Models
{
    /// <summary>
    /// 通知マスターモデル
    /// </summary>
    public class M_NotificationModel : CommonModel
    {
        public int NotificationID { get; set; }
        public string NotificationContent { get; set; } = string.Empty;
        public DateTime? NotificationDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
