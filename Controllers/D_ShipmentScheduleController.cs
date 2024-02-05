using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class D_ShipmentScheduleController : Controller
    {
        public IActionResult Index()
        {
            var model = new D_ShipmentScheduleModel.D_ShipmentSchedule();
            return View(model);
        }
    }
}
