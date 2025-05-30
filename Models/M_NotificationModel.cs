using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 通知マスターモデル
    /// </summary>
    public class M_NotificationModel : CommonModel
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public int NotificationID {  get; set; }

        /// <summary>
        /// 便ID
        /// </summary>
        public int TripID {  get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        public string? TripName { get; set; }

        public string? TripBranchIDAndSeq {  get; set; }

        /// <summary>
        /// 便枝番
        /// </summary>
        public int TripBranchSeq {  get; set; }

        /// <summary>
        /// デポID
        /// </summary>
        public int DepoID {  get; set; }

        /// <summary>
        /// デポ名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 到着荷量下限クラス
        /// </summary>
        public int ArrivalLowerLoadClass {  get; set; }

        /// <summary>
        /// 到着荷量下限の%表示
        /// </summary>
        public string? ArrivalLowerLoadStatus {  get; set; }

        /// <summary>
        /// 出発荷量下限クラス
        /// </summary>
        public int DepartureLowerLoadClass { get; set; }

        /// <summary>
        /// 出発荷量下限の%表示
        /// </summary>
        public string? DepartureLowerLoadStatus { get;set; }

        /// <summary>
        /// 通知開始日時
        /// </summary>
        public DateTime NotificationStartDateTime { get; set; }

        /// <summary>
        /// 通知終了日時
        /// </summary>
        public DateTime NotificationEndDateTime { get; set; }

        public List<string>? NotificationUsersView { get; set; }

        /// <summary>
        /// 通知先ユーザーリスト
        /// </summary>
        public List<R_NotificationUserModel>? NotificationUsers { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted {  get; set; }


        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string? CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy { get; set; }
    }


    // 通知先ユーザー
    public class R_NotificationUserModel : CommonModel
    {
        public int NotificationUserID { get; set; }
        public int NotificationID { get; set; }
        public string ADName {  get; set; }
        public int UserID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class M_NotificationViewModel: CommonModel
    {
        /// <summary>
        /// 通知リスト
        /// </summary>
        public List<M_NotificationModel> M_NotificationList { get; set; }

        /// <summary>
        /// メインデポID
        /// </summary>
        public int MainDepoID {  get; set; }

        /// <summary>
        /// メインデポ名
        /// </summary>
        public string MainDepoName {  get; set; }

        /// <summary>
        /// 通知ID
        /// </summary>
        [Display(Name = "通知ID")]
        public int NotificationID { get; set; }

        /// <summary>
        /// 便ID
        /// </summary>
        public int TripID { get; set; }


        /// <summary>
        ///  便名称選択肢リスト
        /// </summary>
        [Display(Name = "便名称")]
        public List<SelectListItem> TripNameSelectList { get; set; }


        /// <summary>
        /// 便枝番選択肢リスト
        /// </summary>
        [Display(Name = "便枝番")]
        public List<SelectListItem> TripBranchSeqSelectList { get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Display(Name = "便名称")]
        public string? TripName { get; set; }

        public string? TripBranchIDAndSeq { get; set; }

        /// <summary>
        /// 便枝番
        /// </summary>
        public int TripBranchSeq { get; set; }

        /// <summary>
        /// デポID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// デポ名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 到着荷量下限クラス
        /// </summary>
        [Display(Name = "到着荷量下限")]
        public int ArrivalLowerLoadClass { get; set; }

        /// <summary>
        /// 到着荷量下限の%表示
        /// </summary>
        public string? ArrivalLowerLoadStatus { get; set; }

        /// <summary>
        /// 出発荷量下限クラス
        /// </summary>
        [Display(Name = "出発荷量下限")]
        public int DepartureLowerLoadClass { get; set; }

        /// <summary>
        /// 出発荷量下限の%表示
        /// </summary>
        public string? DepartureLowerLoadStatus { get; set; }

        /// <summary>
        /// 通知開始日時
        /// </summary>
        [Display(Name = "通知開始日時")]
        public DateTime NotificationStartDateTime { get; set; }

        /// <summary>
        /// 通知終了日時
        /// </summary>
        [Display(Name = "通知終了日時")]
        public DateTime NotificationEndDateTime { get; set; }

        public List<string>? NotificationUsersView { get; set; }

        /// <summary>
        /// 通知先ユーザーリスト
        /// </summary>
        public List<R_NotificationUserModel>? NotificationUsers { get; set; }
    }

    public class M_NotificationRegisterViewModel : CommonModel
    {
        /// <summary>
        ///  便名称選択肢リスト
        /// </summary>
        [Display(Name = "便名称")]
        public List<SelectListItem> TripNameSelectList { get; set; }

        public int TripID {  get; set; }

        /// <summary>
        /// 便枝番選択肢リスト
        /// </summary>
        [Display(Name = "便枝番")]
        public List<SelectListItem> TripBranchSeqSelectList { get; set; }


        public string? TripBranchIDAndSeq { get; set; }

        public int TripBranchSeq {  get; set; }

        /// <summary>
        /// 到着予定時間
        /// </summary>
        public DateTime ArrivalScheduledTime { get; set; }

        /// <summary>
        /// 出発予定時間
        /// </summary>
        public DateTime DepartureScheduledTime { get; set; }

        /// <summary>
        /// 適用開始日時
        /// </summary>
        public DateTime ApplicableStartDateTime {  get; set; }

        /// <summary>
        /// 適用終了日時
        /// </summary>
        public DateTime ApplicableEndDateTime {  get; set; }

        /// <summary>
        /// 到着荷量下限
        /// </summary>
        [Display(Name = "到着荷量下限")]
        public int ArrivalLowerLoadClass {  get; set; }

        /// <summary>
        /// 出発荷量下限
        /// </summary>
        [Display(Name = "出発荷量下限")]
        public int DepartureLowerLoadClass { get; set; }

        /// <summary>
        /// 通知開始日時
        /// </summary>
        [Display(Name = "通知開始日時")]
        public DateTime NotificationStartDateTime { get; set; }

        /// <summary>
        /// 通知終了日時
        /// </summary>
        [Display(Name = "通知終了日時")]
        public DateTime NotificationEndDateTime { get; set; }

        /// <summary>
        /// メール受信者リスト
        /// </summary>
        [Display(Name = "メール受信者")]
        public List<R_NotificationUserModel> NotificationUserList {  get; set; }
    }
}
