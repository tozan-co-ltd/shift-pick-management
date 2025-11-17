namespace ai_truck_load_measurement.Models
{
    public class ManagementPortalModel : CommonModel
    {
    }

    public class CountNonTripNameRemarkModel : CommonModel
    {
        public string Remark { get; set; }
        public int RemarkCount { get; set; }
        public double RemarkPercentage { get; set; }
        public int DepoID { get; set; }
        public string DepoName { get; set; }
    }

    public class RemarksAndCountNonTripNameRemarkModel
    {
        public List<string> AllRemarks { get; set; }
        public List<CountNonTripNameRemarkModel> NonTripNameRemarks { get; set; }
    }
}
