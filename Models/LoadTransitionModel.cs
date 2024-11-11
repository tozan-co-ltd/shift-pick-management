using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadTransitionModel : LoadRecordModel
    {
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
