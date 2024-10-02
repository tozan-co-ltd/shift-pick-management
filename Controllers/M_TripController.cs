using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            M_TripModel model = new();

            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 車両マスター情報取得SQL作成
                var sql = M_TripConnectController.CreateSQLToSelectMTrips();
                // DB接続
                IEnumerable<M_TripModel> tripList = M_TripConnectController.ConnectMTrips(sql, "AI-truck-load-measurement_test");

                model.M_TripList = tripList.ToPagedList();

                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
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
