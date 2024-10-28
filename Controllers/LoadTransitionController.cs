using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadTransitionModel();
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreatSQLToSelectTripNameFromPeriod(oneWeekAgo, today);
                // DB接続
                List<SelectListItem> tripRecordList = LoadTransitionConnectController.ConnectTTripRecordsForTripName(sql, "AI-truck-load-measurement_test");

                model.TripNameList = tripRecordList;
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }
    }
}
