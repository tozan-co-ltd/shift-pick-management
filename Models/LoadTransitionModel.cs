using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadTransitionModel : CommonModel
    {
        public int TripRecordID {  get; set; }
        public string? TripName {  get; set; }
        public string? TripBranchSeq {  get; set; }
        public string? DriverName {  get; set; }
        public int StationID {  get; set; }
        public int TruckNumber {  get; set; }
        public string? IdentifyNumber {  get; set; }
        public DateTime ArrivalScheduledTime { get; set; }
        public DateTime DepartureScheduledTime { get; set; }
        public DateTime WorkDay {  get; set; }
        public DateTime ArrivedAt { get; set; }
        public DateTime DepartedAt { get; set; }
        public int ArrivalLoadClass {  get; set; }
        public int DepartureLoadClass {  get; set; }
        public string? ArrivalLoadStatus {  get; set; }
        public string? DepartureLoadStatus {  get; set; }
        public int RevisionArrivalLoadClass { get; set; }
        public int RevisionDepartureLoadClass { get; set; }
        public string? ArrivalLoadImgPath {  get; set; }
        public string? DepartureLoadImgPath { get;set; }
        // 便実績リスト
        public IPagedList<LoadTransitionModel>? TripRecordList { get; set; }
        public List<SelectListItem>? TripNameList { get; set; }
        public int AnnotationLoadClass {  get; set; }
        public string? ArrivalDepartureClass {  get; set; }
    }

    public class RequestLoadStatus
    {
        public string? ArrivalLoadStatus { get; set; }
        public string? DepartureLoadStatus { get; set; }
        public DateTime WorkDay { get; set; }
    }
}
