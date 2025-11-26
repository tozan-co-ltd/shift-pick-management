namespace ai_truck_load_measurement.Models
{
    public class NonDepartedAtRecordModel : CommonModel
    {
        public DateTime WorkDay { get; set; }
        public int DepoID { get; set; }
        public string DepoName { get; set; }
        public int NullCount { get; set; }
        public int NoIdentifyNumberNullCount { get; set; }
        public int TotalNullCount {  get; set; }
        public int TripCount { get; set; }
    }

    public class NonDepartedAtRecordViewModel : CommonModel
    {
        // メインデポID
        public int MainDepoID { get; set; }
        // メインデポ名
        public string? MainDepoName { get; set; }
        // ユーザー名
        public string? UserName { get; set; }
    }
}
