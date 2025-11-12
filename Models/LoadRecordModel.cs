using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadRecordModel : CommonModel
    {
        // 便実績ID
        public int TripRecordID { get; set; }
        // 便名称
        public string? TripName { get; set; }
        // 便枝番  
        public string? TripBranchSeq { get; set; }
        // 乗務員
        public string? DriverName { get; set; }
        // ステーションID
        public int StationID { get; set; }
        // ステーション名
        public string? StationName { get; set; }
        // 車両番号
        public string? TruckNumber { get; set; }
        // 識別番号
        public string? IdentifyNumber { get; set; }
        // 到着予定時間
        public DateTime ArrivalScheduledTime { get; set; }
        // 出発予定時間
        public DateTime DepartureScheduledTime { get; set; }
        // 稼働日
        public DateTime WorkDay { get; set; }
        // 到着日時
        public DateTime ArrivedAt { get; set; }
        // 出発日時
        public DateTime DepartedAt { get; set; }
        // 到着荷量
        public int ArrivalLoadClass { get; set; }
        // 到着荷量の%表示
        public string? ArrivalLoadStatus { get; set; }
        // 出発荷量
        public int DepartureLoadClass { get; set; }
        // 出発荷量の%表示
        public string? DepartureLoadStatus { get; set; }
        // 訂正後到着荷量
        public int RevisionArrivalLoadClass { get; set; }
        // 訂正後到着荷量の%表示
        public string? RevisionArrivalLoadStatus { get; set; }
        // 訂正後出発荷量
        public int RevisionDepartureLoadClass { get; set; }
        // 訂正後出発荷量の%表示
        public string? RevisionDepartureLoadStatus { get; set; }
        // 到着荷量画像パス
        public string? ArrivalLoadImgPath { get; set; }
        // 出発荷量画像パス
        public string? DepartureLoadImgPath { get; set; }
        // メインデポ
        public M_DepoModel? MainDepo {  get; set; }
        // 訂正後荷量クラス
        public int AnnotationLoadClass { get; set; }
        // 訂正後荷量の%表示
        public string? AnnotationLoadStatus { get; set; }
        // 到着か出発か
        public string? ArrivalDepartureClass { get; set; }
        // 便名称のリスト
        public List<SelectListItem>? TripNameList { get; set; }
        // 選択された便名称
        public string? SelectedTripName { get; set; }
        // 昼勤開始時間
        public DateTime? DayShiftStartTime { get; set; }
        // デポID
        public int DepoID {  get; set; }
        // デポ名
        public string? DepoName { get; set; }
        // タグ
        public string? Tag { get; set; }
        // 備考
        public string? Remark { get; set; }

        // 便実績リスト
        public IPagedList<LoadRecordModel>? TripRecordList { get; set; }
        // 管理権限区分
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

    public class PivotStatusModel : CommonModel
    {
        public List<string> HeaderColumuns { get; set; }
        public string YColumnName {  get; set; }
        public List<string> XColumnNames {  get; set; }
        public string ValueColumnNames {  get; set; }
    }

}
