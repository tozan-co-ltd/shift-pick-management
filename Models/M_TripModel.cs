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
        public int TripId { get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Required]
        public string? TripName { get; set; }

        /// <summary>
        /// 乗務員
        /// </summary>
        public string? DriverName {  get; set; }

        /// <summary>
        /// 車両番号
        /// </summary>
        public int TruckNumber {  get; set; }

        /// <summary>
        /// 識別番号
        /// </summary>
        public int IdentifyNumber {  get; set; }

        /// <summary>
        /// 昼勤開始時間  
        /// </summary>
        public TimeOnly DayShiftStartTime {  get; set; }

        /// <summary>
        ///  適用開始日時
        /// </summary>
        public DateTime ApplicableStartDateTime { get; set; }

        /// <summary>
        /// 適用終了日時
        /// </summary>
        public DateTime ApplicableEndDateTime { get;set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt {  get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string? CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UploadedAt {  get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UploadedBy { get; set; }

    }
}
