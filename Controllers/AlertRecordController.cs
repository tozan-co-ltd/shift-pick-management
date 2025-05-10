using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class AlertRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new AlertRecordModel();
            return View(model);
        }
    }
}
