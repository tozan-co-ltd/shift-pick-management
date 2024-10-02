using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

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
                // 車両マスター情報取得
                var sql = M_TruckConnectController.CreateSQLToSelectMTrucks();
                IEnumerable<M_TruckModel> truckList = M_TruckConnectController.ConnectMTrucks(sql, "AI-truck-load-measurement_test"); 
                foreach (var truck in truckList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = Convert.ToString(truck.TruckNumber),
                        Value = Convert.ToString(truck.TruckID),
                        Selected = false
                    };

                    model.TruckSelectList.Add(menuItem);
                }
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 車両マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_TripModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 便名称が重複している便履歴の取得
                var duplicateMTripNameSql = M_TripConnectController.CreateSQLToSelectDuplicateMTripName(model);
                var duplicateMTripNameList = M_TripConnectController.ConnectMTrips(duplicateMTripNameSql, "AI-truck-load-measurement_test");

                // 適用期間重複チェック
                bool isDupulicatedApplicablePeriod = false;
                foreach (var item in duplicateMTripNameList)
                {
                    var startTime = item.ApplicableStartDateTime;
                    var endTime = item.ApplicableEndDateTime;
                    var modelStartTime = model.ApplicableStartDateTime;
                    var modelEndTime = model.ApplicableEndDateTime;
                    if (modelEndTime > startTime && endTime > modelStartTime)
                    {
                        isDupulicatedApplicablePeriod = true;
                    }
                }
                if (isDupulicatedApplicablePeriod)
                {
                    // log取得
                     errorMessage = "E1012: " + ErrorMessagesResources.E1012;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }


                M_TripConnectController.InsertMTrip(model, user);
                // log取得
                _logger.Info($"車両マスター登録成功 便名称:{model.TripName}");
                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
        }
    }
}
