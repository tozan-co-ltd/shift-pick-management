using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class TruckRecordsOutputController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
