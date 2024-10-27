using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : Controller
    {
        public IActionResult Index()
        {
            var model = new LoadTransitionModel();
            return View(model);
        }
    }
}
