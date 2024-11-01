using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadTransitionModel : LoadRecordModel
    {
        // 便名称のリスト
        public List<SelectListItem>? TripNameList { get; set; }
        // 選択された便名称
        public string? SelectedTripName {  get; set; }
    }

    /// <summary>
    /// 検索された便実績データ
    /// </summary>
    public class RequestLoadStatus
    {
        // 到着荷量の%表示
        public string? ArrivalLoadStatus { get; set; }
        // 出発荷量の%表示
        public string? DepartureLoadStatus { get; set; }
        // 稼働日
        public DateTime WorkDay { get; set; }
    }

}
