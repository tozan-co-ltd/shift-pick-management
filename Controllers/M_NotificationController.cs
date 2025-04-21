using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class M_NotificationController : BaseController
    {
        public IActionResult Index()
        {
            var model = new M_NotificationViewModel();
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new M_NotificationRegisterViewModel();
            return View(model);
        }
    }
}
