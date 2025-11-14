using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class ManagementPortalController : BaseController
    {
        public IActionResult Index()
        {
            var model = new ManagementPortalModel();
            return View(model);
        }
    }
}
