using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadAverageController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
