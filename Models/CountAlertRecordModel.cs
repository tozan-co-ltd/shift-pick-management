namespace ai_truck_load_measurement.Models
{
    public class CountAlertRecordModel : CommonModel
    {
        public int EarlyArriveCount {  get; set; }
        public int LateArriveCount { get; set; }
        public int EarlyDepartCount {  get; set; }
        public int LateDepartCount { get; set; }
        public int ArrivalLoadCount {  get; set; }
        public int DepartureLoadCount {  get; set; }
        public int AllRecordCount {  get; set; }
        public string? SelectedTripName {  get; set; }
        public string? TripName {  get; set; }
        public string? TripBranchSeq {  get; set; }

    }
}
