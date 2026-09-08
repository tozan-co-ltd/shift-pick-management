namespace shift_pick_management.Models
{
    public class CountAlertRecordModel : CommonModel
    {
        public string? SelectedTripName { get; set; }
        public int EarlyArriveCount {  get; set; }
        public int LateArriveCount { get; set; }
        public int EarlyDepartCount {  get; set; }
        public int LateDepartCount { get; set; }
        public int ArrivalLoadCount {  get; set; }
        public int DepartureLoadCount {  get; set; }
        public int AllRecordCount {  get; set; }
        public int TripRecordCount { get; set; }

    }

    public class CountTripRecordModel : CommonModel
    {
        public  int TripID { get; set; }
        public string? TripName { get; set; }
        public int TripBranchSeq { get; set; }
        public int TripRecordCount { get; set; }
    }
}
