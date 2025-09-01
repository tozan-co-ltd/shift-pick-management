namespace ai_truck_load_measurement.Models
{
    public class LoadProgressModel : CommonModel
    {
        // 便実績リスト
        public List<LoadRecordModel> LoadRecords { get; set; }
        // 便予定リスト
        public List<M_TripBranchNumberModel> TripBranchNumbers { get; set; }
        // 稼働日
        public DateTime WorkDay {  get; set; }
        // 現在時刻
        public DateTime WorkTime { get; set; }
        // 選択デポ
        public M_DepoModel SelectedDepo { get; set; }
    }

    public class TruckExistModel : CommonModel
    {
        public int StationID { get; set; }
        public bool TruckExist { get; set; }
    }
}
