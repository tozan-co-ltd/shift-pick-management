using System.ComponentModel.DataAnnotations;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 便マスターのモデル
    /// </summary>
    public class M_TripModel: CommonModel
    {
        /// <summary>
        /// 車両リスト
        /// </summary>
        public IPagedList<M_TripModel>? M_TripList { get; set; }

        /// <summary>
        /// 便ID
        /// </summary>
        [Display(Name = "便ID")]
        public int TripID { get; set; }

        /// <summary>
        /// 便履歴ID
        /// </summary>
        public int TripHistoryID {  get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Display(Name = "便名称")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? TripName { get; set; }

        /// <summary>
        /// 乗務員
        /// </summary>
        [Display(Name = "乗務員")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DriverName {  get; set; }

        /// <summary>
        /// 車両ID
        /// </summary>
        public int TruckID {  get; set; }

        /// <summary>
        /// 車両番号
        /// </summary>
        [Display(Name = "車両番号")]
        public int TruckNumber {  get; set; }

        /// <summary>
        /// 識別番号
        /// </summary>
        [Display(Name = "識別番号")]
        public int IdentifyNumber {  get; set; }

        /// <summary>
        /// 昼勤開始時間  
        /// </summary>
        [Display(Name = "昼勤開始時間")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Range(typeof(DateTime), "00:00","23:59", ErrorMessage = "hh:mmで入力してください。")]
        public DateTime DayShiftStartTime {  get; set; }

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
        public DateTime ApplicableEndDateTime { get;set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        public DateTime CreatedAt {  get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        public DateTime UpdatedAt {  get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }

        public List<SelectListItem> TruckSelectList { get; set; } = new List<SelectListItem>();

        // 適用終了日時を過ぎた便を表示するチェックボックスの入力
        public bool IsCheckedBeforeApplicablePeriod {  get; set; } = false;
    }
}
