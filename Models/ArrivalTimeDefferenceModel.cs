namespace ai_truck_load_measurement.Models
{
    public class ArrivalTimeDefferenceModel
    {
        public int TripID {  get; set; }
       public string TripName { get; set; }
        public int TripBranchSeq {  get; set; }
        public DateTime ArrivalScheduledTime {  get; set; }
        public DateTime ArrivedAt {  get; set; }
        public DateTime WorkDay {  get; set; }
        public int ArrivalTimeDeff {  get; set; }

    }
}
