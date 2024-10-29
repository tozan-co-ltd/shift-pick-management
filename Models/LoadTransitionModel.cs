using Microsoft.AspNetCore.Mvc.Rendering;

namespace ai_truck_load_measurement.Models
{
    public class LoadTransitionModel : CommonModel
    {
        public string? TripName {  get; set; }
        public int TripBranchSeq {  get; set; }
        public string? DriverName {  get; set; }
        public int StationID {  get; set; }
        public string? TruckNumber {  get; set; }
        public string? IdentifyNumber {  get; set; }
        public DateTime? ArrivalScheduledTime { get; set; }
        public DateTime? DepartureScheduledTime { get; set; }
        public DateTime? WorkDay {  get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? DepartedAt { get; set; }
        public int ArrivalLoadClass {  get; set; }
        public int DepartureLoadClass {  get; set; }
        public string? ArrivalLoadStatus {  get; set; }
        public string? DepartureLoadStatus {  get; set; }
        public int RevisionArrivalLoadClass { get; set; }
        public int RevisionDepartureLoadClass { get; set; }
        public string? ArrivalLoadImgPath {  get; set; }
        public string? DepartureLoadImgPath { get;set; }
        public List<LoadTransitionModel>? LoadTransitionList { get; set; }
        public List<SelectListItem>? TripNameList { get; set; }
    }

    public class RequestLoadStatus
    {
        public string? ArrivalLoadStatus { get; set; }
        public string? DepartureLoadStatus { get; set; }
        public DateTime? WorkDay { get; set; }
    }
}
