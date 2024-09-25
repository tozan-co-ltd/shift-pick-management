using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using ai_truck_load_measurement.ConnectControllers;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 車両マスター画面
    /// </summary>
    public class M_TruckController : BaseController
    {
        /// <summary>
        /// 車両マスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_TruckModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター情報取得SQL作成
                var sql = M_TruckConnectController.CreateSQLToSelectMTrucks();
                // DB接続
                IEnumerable<M_TruckModel> demoList = M_TruckConnectController.ConnectMTrucks(sql, "AI-truck-load-measurement_test");

                model.M_TruckList = demoList.ToPagedList();

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
