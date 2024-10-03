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

            // 適用期間外のデータが必要か
            bool beforePeriod = false;
            
            try
            {
                // 車両マスター情報取得SQL作成
                var sql = M_TripConnectController.CreateSQLToSelectMTrips(beforePeriod);
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
        /// 便情報テーブル非同期更新用
        /// </summary>
        /// <param name="beforePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(bool beforePeriod)
        {
            var searchData = string.Empty;
            List<M_TripModel> tripList = new();
            try
            {
                // 車両マスター情報取得SQL作成
                var sql = M_TripConnectController.CreateSQLToSelectMTrips(beforePeriod);
                // DB接続
                tripList = M_TripConnectController.ConnectMTrips(sql, "AI-truck-load-measurement_test");
                if (tripList.Count > 0)
                {
                    foreach (var item in tripList)
                    {
                        searchData += $@"
                            <tr>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnEditClick('@item.TripHistoryID')"" data-id=""@item.TripHistoryID"" data-toggle=""modal"" data-target=""#edit-modal"">
                                        <i class=""fa-solid fa-pen""></i>
                                    </a>
                                </td>
                                <td>{@item.TripID}</td>
                                <td>{@item.TripName}</td>
                                <td>{@item.DriverName}</td>
                                <td>{@item.TruckNumber}</td>
                                <td>{@item.IdentifyNumber}</td>
                                <td>{@item.DayShiftStartTime.ToString("HH:mm")}</td>
                                <td>{@item.ApplicableStartDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.ApplicableEndDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.UpdatedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.UpdatedBy}</td>
                            </tr>
                    ";
                    }
                }
                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Content(errorMessage);
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

                // 適用期間重複チェック
                var isDupulicated = IsDupulicatedApplicablePeriod(model);
                if (isDupulicated)
                {
                    // log取得
                     errorMessage = "E1012: " + ErrorMessagesResources.E1012;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 便マスター登録
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

        /// <summary>
        /// 便マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_TripModel model)
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
                    _logger.Error($"車両マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 適用期間重複チェック
                var isDupulicated = IsDupulicatedApplicablePeriod(model);
                if (isDupulicated)
                {
                    // log取得
                    errorMessage = "E1012: " + ErrorMessagesResources.E1012;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 便マスター更新
                M_TripConnectController.UpdateMTrip(model, user);

                // log取得
                _logger.Info($"便マスター更新成功 便名称:{model.TripName}");

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

        /// <summary>
        /// 適用期間重複チェック
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsDupulicatedApplicablePeriod(M_TripModel model)
        {
            // 便名称が重複している便履歴の取得
            var duplicateMTripNameSql = M_TripConnectController.CreateSQLToSelectDuplicateMTripName(model);
            var duplicateMTripNameList = M_TripConnectController.ConnectMTrips(duplicateMTripNameSql, "AI-truck-load-measurement_test");

            // 適用期間重複チェック
            bool isDupulicated = false;
            foreach (var item in duplicateMTripNameList)
            {
                var startTime = item.ApplicableStartDateTime;
                var endTime = item.ApplicableEndDateTime;
                var modelStartTime = model.ApplicableStartDateTime;
                var modelEndTime = model.ApplicableEndDateTime;
                if (modelEndTime > startTime && endTime > modelStartTime)
                {
                    isDupulicated = true;
                    break;
                }
            }
            return isDupulicated;
        }
    }
}
