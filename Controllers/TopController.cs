using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using static ai_truck_load_measurement.Models.TopModel;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// トップ画面
    /// </summary>
    public class TopController : BaseController
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        public IActionResult Index()
        {
            TopModel topModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // 最新のステーション状況取得SQL作成
                var latestStationStatusSQL = TopConnectController.CreateSQLToSelectLatestStationStatus();
                // 最新のステーション状況取得
                IEnumerable<TopModel> latestStationStatusList = TopConnectController.ConnectTops(latestStationStatusSQL, "AI-truck-load-measurement_test");
                // トラック有無取得SQL作成
                // トラック有無取得
                // 取得値の変換
                // ステーションの画像取得

                return View(topModel);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(topModel);
            }
        }

    }
}
