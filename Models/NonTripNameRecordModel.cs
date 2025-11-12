namespace ai_truck_load_measurement.Models
{
    public class NonTripNameRecordModel : CommonModel
    {
        public int TripRecordID { get; set; }
        public string IdentifyNumber {  get; set; }
        public DateTime ArrivedAt {  get; set; }
        public string? GuessTripName {  get; set; }
        public string? NearestArrivaLScheduledTime { get; set; }
        public string? ArrivalTimeDefference { get; set; }
        public string? GuessTripBranchNumber {  get; set; }
        public string? Remarks { get; set; }
    }

    public class NonTripNameRecordViewModel : CommonModel
    {
        public List<NonTripNameRecordModel> NonTripNameRecords { get; set; }
        // メインデポID
        public int MainDepoID { get; set; }
        // メインデポ名
        public string? MainDepoName { get; set; }
        // ユーザー名
        public string? UserName { get; set; }
    }
}
