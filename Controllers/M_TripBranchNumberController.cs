using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using MathNet.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripBranchNumberController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 便枝番マスター画面表示
        /// </summary>
        /// <param name="tripId">便ID</param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        public IActionResult Index(int tripId, bool isChecked)
        {
            M_TripBranchNumberModel model = new();

            model.TripID = tripId;
            model.IsCheckedBeforeApplicablePeriod = isChecked;
            model.TripName = M_TripBranchNumberConnectController.ConnectMTripForTripNameFromTripID(tripId);

            try
            {
                // 便マスター情報取得SQL作成
                var sql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(tripId, isChecked);
                // DB接続
                IEnumerable<M_TripBranchNumberModel> tripList = M_TripBranchNumberConnectController.ConnectMTripBranchNumbers(sql);

                model.M_TripBranchNumberList = tripList.ToPagedList();

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
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(int tripId, bool isBeforeApplicablePeriod, bool isAppearedBranchSeq, DateTime refferenceDate )
        {
            var searchData = string.Empty;
            try
            {
                // 便マスター情報取得SQL作成
                var sql = "";
                if (isAppearedBranchSeq)
                {
                    sql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbersWithBranceSeq(tripId, refferenceDate);
                }
                else
                {
                    sql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(tripId, isBeforeApplicablePeriod);
                }

                // DB接続
                List<M_TripBranchNumberModel> tripBranchNumberList = M_TripBranchNumberConnectController.ConnectMTripBranchNumbers(sql);
                // 枝連番列を追加
                if (isAppearedBranchSeq)
                {
                    tripBranchNumberList = AddTripBranchSeq(tripBranchNumberList);
                }


                // 新しい便情報テーブルのhtml作成
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th width=""40""></th>
                                    <th class=""font-weight-bold"">便枝番ID</th>";
                if (isAppearedBranchSeq)
                {
                    searchData += $@"<th class=""font-weight-bold"">枝連番</th>";

                }
                 searchData +=   $@"<th class=""font-weight-bold"">到着予定時間</th>
                                    <th class=""font-weight-bold"">出発予定時間</th>
                                    <th class=""font-weight-bold"">適用開始日時</th>
                                    <th class=""font-weight-bold"">適用終了日時</th>
                                    <th class=""font-weight-bold"">更新日時</th>
                                    <th class=""font-weight-bold"">更新者</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                if (tripBranchNumberList.Count() > 0)
                {
                    foreach (var item in tripBranchNumberList)
                    {
                        searchData += $@"
                                <tr>
                                    <td>
                                        <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                           onclick=""OnEditClick('{@item.TripBranchNumberID}')"" data-id=""@item.TripHistoryID"" data-toggle=""modal"" data-target=""#edit-modal"">
                                            <i class=""fa-solid fa-pen""></i>
                                        </a>
                                    </td>
                                    <td>{@item.TripBranchNumberID}</td>
                        ";
                        if (isAppearedBranchSeq)
                        {
                            searchData += $@"<td>{item.TripBranchSeq}</td>";

                        }
                        searchData += $@"
                                    <td>{@item.ArrivalScheduledTime.ToString("HH:mm")}</td>
                                    <td>{@item.DepartureScheduledTime.ToString("HH:mm")}</td>
                                    <td>{@item.ApplicableStartDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                    <td>{@item.ApplicableEndDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                    <td>{@item.UpdatedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                    <td>{@item.UpdatedBy}</td>
                                </tr>
                        ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

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
        /// 便枝番リストに枝連番を追加する
        /// </summary>
        /// <param name="tripBranchNumberList">枝連番を追加する対象便枝番リスト</param>
        /// <returns></returns>
        private List<M_TripBranchNumberModel> AddTripBranchSeq(List<M_TripBranchNumberModel> tripBranchNumberList)
        {
            var countBeforeShiftStartTimeRow = 0; // 到着予定時間が昼勤開始時間より早い行の数
            var tripBranchSeq = 1; // 枝連番

            for (int i = 0; i < tripBranchNumberList.Count; i++)
            {

                var tripBranchNumber = tripBranchNumberList[i];
                var dayShiftStartTime = tripBranchNumber.DayShiftStartTime;
                var arrivalScheduledTime = tripBranchNumber.ArrivalScheduledTime;

                // 到着予定時間が昼勤開始時間以降のデータの場合、枝連番付与
                // それ以外の場合、昼勤開始時間以前の行数のカウントを1増やす
                if (arrivalScheduledTime > dayShiftStartTime)
                {
                    tripBranchNumber.TripBranchSeq = tripBranchSeq;
                    tripBranchSeq++;
                }
                else
                {
                    countBeforeShiftStartTimeRow++;
                }
            }

            // 到着予定時間が昼勤開始時間以前のデータに枝連番付与
            if (countBeforeShiftStartTimeRow > 0)
            {
                for (int i = 0; i < countBeforeShiftStartTimeRow; i++)
                {
                    var tripBranchNumber = tripBranchNumberList[i];
                    tripBranchNumber.TripBranchSeq = tripBranchSeq;
                    tripBranchSeq++;
                }
            }

            return tripBranchNumberList;
        }

        /// <summary>
        /// 便枝番マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_TripBranchNumberModel model)
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
                    _logger.Error($"便マスター更新失敗 {errorMessage}");

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
                M_TripBranchNumberConnectController.UpdateMTripBranchNumber(model, user);

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
        /// 便枝番マスター登録画面表示
        /// </summary>
        /// <param name="id">便履歴ID、新しい適用期間の作成時に値が入る</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register(bool isChecked, int id, string tripName)
        {
            M_TripBranchNumberModel model = new();
            try
            {
                model.TripID = id;
                model.TripName = tripName;

                // 適用終了日時を過ぎた便を表示するチェックボックスの入力
                model.IsCheckedBeforeApplicablePeriod = isChecked;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 便枝番マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_TripBranchNumberModel model)
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
                    _logger.Error($"便枝番マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 適用期間と到着、出発予定時間の重複チェック
                var isDupulicated = IsDupulicatedApplicablePeriod(model);
                if (isDupulicated)
                {
                    // log取得
                    errorMessage = "E1012: " + ErrorMessagesResources.E1012;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 便マスター登録
                M_TripBranchNumberConnectController.InsertMTripBranchNumber(model, user);

                // log取得
                _logger.Info($"便マスター登録成功 便名称:{model.TripName}");
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
        /// 適用期間と到着、出発予定時間の重複チェック
        /// </summary>
        /// <param name="model">便枝番マスターモデル</param>
        /// <returns></returns>
        private bool IsDupulicatedApplicablePeriod(M_TripBranchNumberModel model)
        {
            // 便IDが重複している便履歴の取得
            var duplicateTripIDSql = M_TripBranchNumberConnectController.CreateSQLToSelectTimesFromDuplicateTripID(model);
            var duplicateMTripNameList = M_TripBranchNumberConnectController.ConnectMTripBranchNumbers(duplicateTripIDSql);

            // 適用期間重複チェック
            bool isDupulicated = false;
            foreach (var item in duplicateMTripNameList)
            {
                var arrivalTime = item.ArrivalScheduledTime;
                var departureTime = item.DepartureScheduledTime;
                var startTime = item.ApplicableStartDateTime;
                var endTime = item.ApplicableEndDateTime;
                var modelArrivalTime = DateTime.Parse($"1900/01/01 {model.RegistArrivalScheduledTime}:00");
                var modelDepartureTime = DateTime.Parse($"1900/01/01 {model.RegistDepartureScheduledTime}:00");
                var modelStartTime = model.ApplicableStartDateTime;
                var modelEndTime = model.ApplicableEndDateTime;
                if (modelDepartureTime > arrivalTime && departureTime > modelArrivalTime &&
                    modelEndTime > startTime && endTime > modelStartTime)
                {
                    isDupulicated = true;
                    break;
                }
            }
            return isDupulicated;
        }
    }
}
