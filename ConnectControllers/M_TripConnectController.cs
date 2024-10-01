using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_TripConnectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
