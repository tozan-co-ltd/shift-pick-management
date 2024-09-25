using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TruckController : Controller
    {
        public IActionResult Index()
        {
            M_TruckModel model = new();
            return View(model);
        }
    }
}
