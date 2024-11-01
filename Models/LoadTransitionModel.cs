using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class LoadTransitionModel : LoadRecordModel
    {
        public List<SelectListItem>? TripNameList { get; set; }
        public string? SelectedTripName {  get; set; }
    }

    public class RequestLoadStatus
    {
        public string? ArrivalLoadStatus { get; set; }
        public string? DepartureLoadStatus { get; set; }
        public DateTime WorkDay { get; set; }
    }

}
