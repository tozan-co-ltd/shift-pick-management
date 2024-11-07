using Microsoft.AspNetCore.Mvc;
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
        // 車両番号
        public int TruckNumber { get; set; }
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
        // 便実績リスト
        public IPagedList<LoadRecordModel>? TripRecordList { get; set; }
        // 訂正後荷量クラス
        public int AnnotationLoadClass { get; set; }
        // 到着か出発か
        public string? ArrivalDepartureClass { get; set; }
        // 選択された便名称
        public string? SelectedTripName { get; set; }
    }
}
