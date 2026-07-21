namespace ai_truck_load_measurement.Models
{
    public class AIModelDescriptionModel
    {
        public int AIModelID { get; set; }
        public string AIModelName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
    }
    public class AIModelDescriptionViewModel : CommonModel
    {
        public List<AIModelDescriptionModel> DescriptionList { get; set; }
    }
}
