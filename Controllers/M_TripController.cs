using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripController : Controller
    {
        public IActionResult Index()
        {
            M_TripModel model = new();
            return View(model);
        }

        /// <summary>
        /// 便マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_TripModel model = new();
            try
            {
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }
    }
}
