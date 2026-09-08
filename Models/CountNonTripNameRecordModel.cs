namespace shift_pick_management.Models
{
    public class CountNonTripNameRecordModel : CommonModel
    {
        public int TripID {  get; set; }
        public string? IdentifyNumber {  get; set; }
        public string? TripName { get; set; }
        public int NoNameCount {  get; set; }
        public int TripCount {  get; set; }
        public string? DepoName { get; set; }
    }

    public class CountNonTripNameRecordViewModel : CommonModel
    {
        // メインデポID
        public int MainDepoID { get; set; }
        // メインデポ名
        public string? MainDepoName { get; set; }
        // ユーザー名
        public string? UserName { get; set; }
    }
}
