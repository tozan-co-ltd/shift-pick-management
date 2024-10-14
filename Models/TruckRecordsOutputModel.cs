using System.ComponentModel.DataAnnotations;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    public class TruckRecordsOutputModel : CommonModel
    {
        public string? TripName { get; set; }

        public int TripBrunchNumber { get; set; }

        public string? DriverName { get; set; }

        public int StationID { get; set; }

        public int TruckID { get; set; }

        public int IdentifyNumber { get; set; }

        public DateTime ArrivalScheduledTime { get; set; }

        public DateTime DepartureScheduledTime { get; set; }

        public DateTime WorkDay { get; set; }

        public DateTime ArrivedAt { get; set; }

        public DateTime DeparturedAt { get; set; }

        public int ArrivalLoadClass { get; set; }

        public int DepartureLoadClass { get; set; }

        public int RevisionArrivalLoadClass { get; set; }

        public int RevisionDepartureLoadClass { get; set; }

        public string? ArrivalLoadImagePath { get; set; }

        public string? DepartureLoadImagePath { get; set; }
    }
}
