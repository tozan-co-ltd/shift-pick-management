using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spire.Xls;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 便枝番マスター用モデル
    /// </summary>
    public class M_TripBranchNumberModel : CommonModel
    {
        /// <summary>
        /// 便枝番リスト
        /// </summary>
        public IPagedList<M_TripBranchNumberModel>? M_TripBranchNumberList { get; set; }

        /// <summary>
        /// 便ID
        /// </summary>
        [Display(Name = "便ID")]
        public int TripID { get; set; }

        /// <summary>
        /// 便枝番ID
        /// </summary>
        public int TripBranchNumberID { get; set; }

        /// <summary>
        /// 枝連番
        /// </summary>
        public int TripBranchSeq {  get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Display(Name = "便名称")]
        public string? TripName { get; set; }

        /// <summary>
        /// 到着予定時間  
        /// </summary>
        public DateTime ArrivalScheduledTime { get; set; }

        /// <summary>
        /// 到着予定時間登録用
        /// </summary>
        [Display(Name = "到着予定時間")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"^([0-1][0-9]|[2][0-3]):[0-5][0-9]$", ErrorMessage = "hh:mmで入力してください。")]
        public string? RegistArrivalScheduledTime { get; set; }

        /// <summary>
        /// 出発予定時間  
        /// </summary>
        public DateTime DepartureScheduledTime { get; set; }

        /// <summary>
        /// 出発予定時間登録用
        /// </summary>
        [Display(Name = "出発予定時間")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"^([0-1][0-9]|[2][0-3]):[0-5][0-9]$", ErrorMessage = "hh:mmで入力してください。")]
        public string? RegistDepartureScheduledTime { get; set; }

        /// <summary>
        ///  適用開始日時
        /// </summary>
        [Display(Name = "適用開始日時")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public DateTime ApplicableStartDateTime { get; set; }

        /// <summary>
        /// 適用終了日時
        /// </summary>
        [Display(Name = "適用終了日時")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public DateTime ApplicableEndDateTime { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// 昼勤開始時間
        /// </summary>
        public DateTime DayShiftStartTime { get; set; }


        public List<SelectListItem> TruckSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 適用終了日時を過ぎた便を表示するチェックボックスの入力
        /// </summary>
        public bool IsCheckedBeforeApplicablePeriod { get; set; } = false;

    }
}
