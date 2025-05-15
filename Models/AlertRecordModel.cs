using Org.BouncyCastle.Asn1.Mozilla;

namespace ai_truck_load_measurement.Models
{
    public class AlertRecordModel : CommonModel
    {
        public int AlertRecordID { get; set; }
        public int TripRecordID {  get; set; }
        public int NotificationID {  get; set; }
        public int ArrivalLowerLoadClass {  get; set; }
        public string? ArrivalLowerLoadStatus {  get; set; }
        public int DepartureLowerLoadClass { get; set; }
        public string? DepartureLowerLoadStatus { get; set; }
        public List<string>? AlertItems {  get; set; }
        public List<string>? ArrivalAlertItems { get; set; }
        public List<string>? DepartureAlertItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //public string TripName { get; set; }
        //public int TripBranchSeq { get; set; }
        //public string StationName { get; set; }
        //public string DriverName { get; set; }
        //public DateTime WorkDay { get; set; }

    }

    public class AlertRecordModalModel : CommonModel
    {
        public int AlertRecordID { get; set; }
        public string TripName {  get; set; }
        public int TripBranchSeq {  get; set; }
        public string DriverName {  get; set; }
        public string StationName {  get; set; }
        public string TruckNumber {  get; set; }
        public string IdentifyNumber {  get; set; }
        public int ArrivalLoadClass {  get; set; }
        public string ArrivalLoadStatus {  get; set; }
        public int DepartureLoadClass {  get; set; }
        public string DepartureLoadStatus {  get; set; }
        public int ArrivalLowerLoadClass { get; set; }
        public string ArrivalLowerLoadStatus {  get; set; }
        public int DepartureLowerLoadClass { get; set; }
        public string DepartureLowerLoadStatus { get; set; }
        public DateTime ArrivedAt {  get; set; }
        public DateTime DepartedAt { get; set; }
        public DateTime ArrivalScheduledTime {  get; set; }
        public DateTime DepartureScheduledTime {  get; set; }
        public string ArrivalLoadImgPath {  get; set; }
        public string DepartureLoadImgPath { get; set;}
    }

    public class AlertRecordViewModel : CommonModel
    {
        public List<AlertRecordModel> AlertRecordList { get; set; }
        public List<LoadRecordModel> LoadRecordList { get; set; }
        public M_DepoModel MainDepo {  get; set; }
    }
}
