using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
