using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ai_truck_load_measurement.Models
{
    public class EditLoadRecordModel : LoadRecordModel
    {
        /// <summary>
        /// 実績削除フラグ
        /// </summary>
        [Display(Name = "実績削除")]
        public bool IsDeleted {  get; set; }

        public List<SelectListItem> TripBranchNumberSelectList {  get; set; } = new List<SelectListItem>();
        public List<SelectListItem> IdentifyNumberSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> UnLinkedReasonSelectList { get; set; } = new List<SelectListItem>();
        [Display(Name = "到着予定時間")]
        public string? RegistArrivalScheduledTime { get; set; }
        [Display(Name = "出発予定時間")]
        public string? RegistDepartureScheduledTime { get; set; }
    }
}
