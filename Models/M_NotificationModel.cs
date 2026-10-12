using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace shift_pick_management.Models
{
    /// <summary>
    /// 通知マスターモデル
    /// </summary>
    public class M_NotificationModel : CommonModel
    {
        public int NotificationID { get; set; }
    }
}
