using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadTransitionConnectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
