using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            NewsListViewModel model = new();
            return View(model);
        }
    }
}
