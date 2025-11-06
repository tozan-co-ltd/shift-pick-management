namespace ai_truck_load_measurement.Models
{
    public class NonIdentifyNumberRecordModel
    {
        public DateTime ArrivedAt {  get; set; }
        public DateTime DepartedAt {  get; set; }
        public DateTime WorkDay {  get; set; }
        public string StationName {  get; set; }
        public string ArrivalLoadImgPath {  get; set; }
        public string DepartureLoadImgPath { get; set; }


    }
}
