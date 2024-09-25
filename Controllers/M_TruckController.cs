using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 車両マスター画面
    /// </summary>
    public class M_TruckController : Controller
    {
        /// <summary>
        /// 車両マスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_TruckModel model = new();
            return View(model);
        }

        /// <summary>
        /// 車両マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_TruckModel model = new();
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
