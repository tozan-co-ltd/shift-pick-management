using System.ComponentModel.DataAnnotations;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 便マスターのモデル
    /// </summary>
    public class M_TripModel: CommonModel
    {
        /// <summary>
        /// 便ID
        /// </summary>
        [Display(Name = "便ID")]
        public int TripId { get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Display(Name = "便名称")]
        [Required]
        public string? TripName { get; set; }

        /// <summary>
        /// 乗務員
        /// </summary>
        [Display(Name = "乗務員")]
        public string? DriverName {  get; set; }

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
        public TimeOnly DayShiftStartTime {  get; set; }

        /// <summary>
        ///  適用開始日時
        /// </summary>
        [Display(Name = "適用開始日時")]
        public DateTime ApplicableStartDateTime { get; set; }

        /// <summary>
        /// 適用終了日時
        /// </summary>
        [Display(Name = "適用終了日時")]
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
        public DateTime UploadedAt {  get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UploadedBy { get; set; }

    }
}
