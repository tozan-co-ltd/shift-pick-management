using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class T_TripRecordController : BaseController
    {
        public IActionResult Index()
        {
            T_TripRecordModel model = new();
            // 初期表示の日付を取得
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);

            try
            {
                // 便実績情報取得SQL作成
                var sql = T_TripRecordConnectController.CreatSQLToSelectTripRecord(oneWeekAgo, today);
                // DB接続
                IEnumerable<T_TripRecordModel> tripRecordList =T_TripRecordConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

                model.TripRecordList = tripRecordList.ToPagedList();

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
