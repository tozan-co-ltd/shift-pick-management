using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Mozilla;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// アラート履歴モデル
    /// </summary>
    public class AlertRecordModel : CommonModel
    {
        /// <summary>
        /// アラート履歴ID
        /// </summary>
        public int AlertRecordID { get; set; }

        /// <summary>
        /// 便実績ID
        /// </summary>
        public int TripRecordID {  get; set; }

        /// <summary>
        /// 通知ID
        /// </summary>
        public int NotificationID {  get; set; }

        /// <summary>
        /// 到着荷量下限クラス
        /// </summary>
        public int ArrivalLowerLoadClass {  get; set; }

        /// <summary>
        /// 到着荷量下限％表示
        /// </summary>
        public string? ArrivalLowerLoadStatus {  get; set; }

        /// <summary>
        /// 出発荷量下限クラス
        /// </summary>
        public int DepartureLowerLoadClass { get; set; }

        /// <summary>
        /// 出発荷量下限％表示
        /// </summary>
        public string? DepartureLowerLoadStatus { get; set; }

        /// <summary>
        /// アラート項目
        /// </summary>
        public List<string>? AlertItems {  get; set; }

        /// <summary>
        /// 到着アラート項目
        /// </summary>
        public List<string>? ArrivalAlertItems { get; set; }

        /// <summary>
        /// 出発アラート項目
        /// </summary>
        public List<string>? DepartureAlertItems { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

    }

    /// <summary>
    /// アラート履歴ビュー用モデル
    /// </summary>
    public class AlertRecordViewModel : CommonModel
    {

        /// <summary>
        /// 画面遷移時の通知ID
        /// </summary>
        public int TransitionAlertRecordID { get; set; }

        public bool TransitionIsArrived { get; set; }

        public List<AlertRecordModel> AlertRecordList { get; set; }
        public List<LoadRecordModel> LoadRecordList { get; set; }
        public int MainDepoID {  get; set; }
        public string MainDepoName { get; set; }
    }
}
