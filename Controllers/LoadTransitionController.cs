using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
