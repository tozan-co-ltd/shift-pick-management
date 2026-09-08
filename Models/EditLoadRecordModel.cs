using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace shift_pick_management.Models
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
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? RegistArrivalScheduledTime { get; set; }
        [Display(Name = "出発予定時間")]
        public string? RegistDepartureScheduledTime { get; set; }
    }

    public class UnlinkedReasonModel
    {
        public int UnlinkedReasonID {  get; set; }
        public string? UnlinkedReasonName {  get; set; }
    }

    public class CorrectionItemModel : CommonModel
    {
        public int CorrectionItemID {  get; set; }
        public int? TripRecordID {  get; set; }
        public bool IsIdentifyNumberChanged {  get; set; }
        public string? BeforeIdentifyNumber {  get; set; }
        public bool IsTripIDChanged {  get; set; }
        public int? BeforeTripID {  get; set; }
        public bool IsTripBranchNumberIDChanged {  get; set; }
        public int? BeforeTripBranchNumberID {  get; set; }
        public bool IsArrivedAtChanged {  get; set; }
        public DateTime BeforeArrivedAt {  get; set; }
        public bool IsDepartedAtChanged {  get; set; }
        public DateTime? BeforeDepartedAt {  get; set; }
        public bool IsArrivalLoadImgPathChanged {  get; set; }
        public string? BeforeArrivalLoadImgPath {  get; set; }
        public bool IsDepartureLoadImgPathChanged { get; set; }
        public string? BeforeDepartureLoadImgPath { get; set; }
        public DateTime CreatedAt {  get; set; }
        public string? CreatedBy {  get; set; }
    }
}
