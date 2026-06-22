using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadRecordModel : CommonModel
    {
        /// <summary>
        /// 便実績ID
        /// </summary>
        public int TripRecordID { get; set; }

        /// <summary>
        /// 便ID
        /// </summary>
        public int TripID {  get; set; }

        /// <summary>
        /// 便名称
        /// </summary>
        [Display(Name = "便名称")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? TripName { get; set; }

        /// <summary>
        /// 便枝番ID
        /// </summary>
        [Display(Name = "便枝番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int TripBranchNumberID {  get; set; }

        /// <summary>
        /// 便枝番  
        /// </summary>
        [Display(Name = "便枝番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? TripBranchSeq { get; set; }

        /// <summary>
        /// 乗務員
        /// </summary>
        [Display(Name = "乗務員")]
        public string? DriverName { get; set; }

        /// <summary>
        /// ステーションID
        /// </summary>
        public int StationID { get; set; }

        /// <summary>
        /// ステーション名
        /// </summary>
        [Display(Name = "ステーション名")]
        public string? StationName { get; set; }

        /// <summary>
        /// 車両ID
        /// </summary>
        public int TruckID {  get; set; }

        /// <summary>
        /// 車両番号
        /// </summary>
        [Display(Name = "車両番号")]
        public string? TruckNumber { get; set; }

        /// <summary>
        /// 識別番号
        /// </summary>
        [Display(Name = "識別番号")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? IdentifyNumber { get; set; }

        /// <summary>
        /// 到着予定時間
        /// </summary>
        [Display(Name = "到着予定時間")]
        public DateTime ArrivalScheduledTime { get; set; }

        /// <summary>
        /// 出発予定時間
        /// </summary>
        [Display(Name = "出発予定時間")]
        public DateTime DepartureScheduledTime { get; set; }

        /// <summary>
        /// 稼働日
        /// </summary>
        public DateTime WorkDay { get; set; }

        /// <summary>
        /// 到着日時
        /// </summary>
        [Display(Name = "到着実績")]
        public DateTime ArrivedAt { get; set; }

        /// <summary>
        /// 出発日時
        /// </summary>
        [Display(Name = "出発実績")]
        public DateTime DepartedAt { get; set; }

        /// <summary>
        /// 到着荷量
        /// </summary>
        public int ArrivalLoadClass { get; set; }

        /// <summary>
        /// 到着荷量の%表示
        /// </summary>
        public string? ArrivalLoadStatus { get; set; }

        /// <summary>
        /// 出発荷量
        /// </summary>
        public int DepartureLoadClass { get; set; }

        /// <summary>
        /// 出発荷量の%表示
        /// </summary>
        public string? DepartureLoadStatus { get; set; }

        /// <summary>
        /// 訂正後到着荷量
        /// </summary>
        public int RevisionArrivalLoadClass { get; set; }

        /// <summary>
        /// 訂正後到着荷量の%表示
        /// </summary>
        [Display(Name = "到着荷量")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? RevisionArrivalLoadStatus { get; set; }

        /// <summary>
        /// 訂正後出発荷量
        /// </summary>
        public int RevisionDepartureLoadClass { get; set; }

        /// <summary>
        /// 訂正後出発荷量の%表示
        /// </summary>
        [Display(Name = "出発荷量")]
        public string? RevisionDepartureLoadStatus { get; set; }

        /// <summary>
        /// 到着荷量画像パス
        /// </summary>
        [Display(Name = "到着荷量画像")]
        public string? ArrivalLoadImgPath { get; set; }

        /// <summary>
        /// 出発荷量画像パス
        /// </summary>
        [Display(Name = "出発荷量画像")]
        public string? DepartureLoadImgPath { get; set; }

        /// <summary>
        /// メインデポ
        /// </summary>
        public M_DepoModel? MainDepo {  get; set; }

        /// <summary>
        /// 訂正後荷量クラス
        /// </summary>
        public int AnnotationLoadClass { get; set; }

        /// <summary>
        /// 訂正後荷量の%表示
        /// </summary>
        public string? AnnotationLoadStatus { get; set; }

        /// <summary>
        /// 到着か出発か
        /// </summary>
        public string? ArrivalDepartureClass { get; set; }

        /// <summary>
        /// 便名称のリスト
        /// </summary>
        public List<SelectListItem>? TripNameList { get; set; }

        /// <summary>
        /// 選択された便名称
        /// </summary>
        public string? SelectedTripName { get; set; }

        /// <summary>
        /// 昼勤開始時間
        /// </summary>
        public DateTime? DayShiftStartTime { get; set; }

        /// <summary>
        /// デポID
        /// </summary>
        public int DepoID {  get; set; }

        /// <summary>
        /// デポ名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// タグ
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// 紐づけ切れ原因
        /// </summary>
        [Display(Name = "紐づけ切れ原因")]
        public string? Remark { get; set; }

        /// <summary>
        /// 紐づけ切れ原因ID
        /// </summary>
        public int UnlinkedReasonID { get; set; }

        /// <summary>
        /// 紐づけ切れ原因
        /// </summary>
        [Display(Name = "紐づけ切れ原因")]
        public string? UnlinkedReasonName { get; set; }

        /// <summary>
        /// AIモデル名
        /// </summary>
        [Display(Name = "AIモデル名")]
        public string? AIModelName {  get; set; }

        /// <summary>
        /// 便実績リスト
        /// </summary>
        public IPagedList<LoadRecordModel>? TripRecordList { get; set; }

        /// <summary>
        /// 管理権限区分
        /// </summary>
        public int AuthorizedKubun {  get; set; }
    }

    public class LoadRecordViewModel: CommonModel
    {
        // 便実績リスト
        public IPagedList<LoadRecordModel>? TripRecordList { get; set; }
        // メインデポID
        public int MainDepoID { get; set; }
        // メインデポ名
        public string? MainDepoName { get; set; }
        // ユーザー名
        public string? UserName { get; set; }
        // 便名称のリスト
        public List<SelectListItem>? TripNameList { get; set; }
        // 選択された便名称
        public string? SelectedTripName { get; set; }
    }
}
