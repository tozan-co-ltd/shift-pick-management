using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using ai_truck_load_measurement.Commons;
using System.Data;
using Dapper;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 便マスター画面表示
        /// </summary>
        /// <param name="isChecked">適用終了日時を過ぎた便を表示するチェックボックスの入力</param>
        /// <returns></returns>
        public IActionResult Index(bool? isChecked)
        {
            M_TripViewModel model = new();

            // 適用期間より前のデータが必要か
            bool isBeforeApplicablePeriod = true;

            // 適用終了日時を過ぎた便を表示するチェックボックスの入力がある場合
            if (isChecked != null)
            {
                model.IsCheckedBeforeApplicablePeriod = isChecked.Value;
            }
            
            try
            {
                // ログインユーザーのメインデポ情報取得
                var user = ClaimsLoginUserData();
                model.MainDepoID = user.MainDepoID;
                model.MainDepoName = user.MainDepoName;
                List<string> depoList = new();
                depoList.Add(user.MainDepoID.ToString());

                IEnumerable<M_TripModel> tripList = new List<M_TripModel>();

                if (depoList.Count > 0)
                {
                    // 便マスター情報取得SQL作成
                    var sql = M_TripConnectController.CreateSQLToSelectMTrips(isBeforeApplicablePeriod, depoList);
                    // DB接続
                    tripList = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
                }

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
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(bool isBeforeApplicablePeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<M_TripModel> tripList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    // 便マスター情報取得SQL作成
                    var sql = M_TripConnectController.CreateSQLToSelectMTrips(isBeforeApplicablePeriod, checkedDepos);
                    // DB接続
                    tripList = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
                }

                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th width=""40""></th>
                                    <th class=""font-weight-bold"">便ID</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">乗務員</th>
                                    <th class=""font-weight-bold"">車両番号</th>
                                    <th class=""font-weight-bold"">識別番号</th>
                                    <th class=""font-weight-bold"">デポ</th>
                                    <th class=""font-weight-bold"">昼勤開始時間</th>
                                    <th class=""font-weight-bold"">適用開始日時</th>
                                    <th class=""font-weight-bold"">適用終了日時</th>
                                    <th class=""font-weight-bold"">更新日時</th>
                                    <th class=""font-weight-bold"">更新者</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // 新しい便情報テーブルのhtml作成
                if (tripList.Count > 0)
                {
                    foreach (var item in tripList)
                    {
                        searchData += $@"
                            <tr>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnEditClick('{@item.TripHistoryID}')"" data-id=""@item.TripHistoryID"" data-toggle=""modal"" data-target=""#edit-modal"">
                                        <i class=""fa-solid fa-pen""></i>
                                    </a>
                                    <a class=""btn btn-icon-split ml-1 mr-1""onclick=""onRegisterClick('M_Trip', {item.TripHistoryID})"">
                                        <i class=""fa-regular fa-copy""></i>
                                    </a>
                                </td>
                                <td><a class=""btn"" href=""M_TripBranchNumber?tripId={item.TripID}&isChecked={isBeforeApplicablePeriod}""><span style=""color:deepskyblue"">{@item.TripID}</a></td>
                                <td>{@item.TripName}</td>
                                <td>{@item.DriverName}</td>
                                <td>{@item.TruckNumber}</td>
                                <td>{@item.IdentifyNumber}</td>
                                <td>{@item.DepoName}</td>
                                <td>{@item.DayShiftStartTime.ToString("HH:mm")}</td>
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
        /// 便マスター登録画面表示
        /// </summary>
        /// <param name="id">便履歴ID、新しい適用期間の作成時に値が入る</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register(bool isChecked, int id)
        {
            M_TripViewModel model = new();
            try
            {   
                // 新しい適用期間の作成の場合
                if (id != 0)
                {
                    // 便履歴IDから便履歴情報取得
                    var tripSql = M_TripConnectController.CreateSQLToSelectMTripHistoryByTripHistoryId(id);
                    List<M_TripModel> tripList = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(tripSql);

                    // 同一便IDのデータが1つだけのとき以外はエラー
                    if (tripList.Count != 1)
                    {
                        ViewData["ErrorMessage"] = "E3004: " + ErrorMessagesResources.E3004;
                        return View(model);
                    }
                    model = SetTripStatusToTripViewModel( tripList[0], model);
                }
                // 車両マスター情報取得
                var truckSql = M_TruckConnectController.CreateSQLToSelectMTrucks();
                IEnumerable<M_TruckModel> truckList = ConnectToSQLServer.ExecuteQueryToList<M_TruckModel>(truckSql);

                // 車両番号のセレクトリスト作成
                SelectListItem firstItem = new()
                {
                    Text = "選択してください",
                    Value = "",
                    Selected = true,
                    Disabled = true
                };
                model.TruckSelectList.Add(firstItem);
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

        private M_TripViewModel SetTripStatusToTripViewModel(M_TripModel tripModel, M_TripViewModel viewModel)
        {
            viewModel.TripName = tripModel.TripName;
            viewModel.DriverName = tripModel.DriverName;
            viewModel.TruckID = tripModel.TruckID;
            viewModel.DayShiftStartTime = tripModel.DayShiftStartTime;
            viewModel.DepoID = tripModel.DepoID;
            viewModel.DepoName = tripModel.DepoName;
            viewModel.ApplicableStartDateTime = tripModel.ApplicableStartDateTime;
            viewModel.ApplicableEndDateTime = tripModel.ApplicableEndDateTime;
            return viewModel;
        }

        /// <summary>
        /// 便マスター登録
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
                M_TripConnectController.InsertMTripAndMTripHistory(model, user);

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
            var duplicateMTripNameSql = M_TripConnectController.CreateSQLToSelectApplicablePeriodFromDuplicateMTripName(model);
            var duplicateMTripNameList = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(duplicateMTripNameSql);

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

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, bool isBeforeApplicablePeriod, DateTime referenceDate, List<string> checkedDepos)
        {
            string? errorMessage;
            try
            {
                DataTable mTripDT = new DataTable(); // 便マスター用データテーブル
                DataTable mTripBranchDT = new DataTable(); // 便枝番マスター用データテーブル
                DataTable mTripBranchConsecutiveDT = new DataTable();

                if (checkedDepos.Count > 0)
                {
                    // 便マスター情報取得
                    var mTripSql = M_TripConnectController.CreateSQLToSelectMTripsForDataTable(isBeforeApplicablePeriod, referenceDate, checkedDepos);
                    mTripDT = ConnectToSQLServer.ConnectToDataTable(mTripSql);

                    // 便枝番マスター情報取得
                    var mTripBranchSql = M_TripConnectController.CreateSQLToSelectMTripBranchesForDataTable(referenceDate, checkedDepos);
                    mTripBranchDT = ConnectToSQLServer.ConnectToDataTable(mTripBranchSql);
                    // 便枝番マスターに枝連番列を追加
                    mTripBranchConsecutiveDT = SortDataTableFromBranchConsecutiveNumber(mTripBranchDT);
                }


                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(gamenName);
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "便マスターシート";
                string sheetNameTwo = "便枝番マスターシート";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(mTripDT, mTripBranchConsecutiveDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);

                        CreateFile.DeleteFile(tmpFilename);

                        return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
                    }
                    else
                    {
                        // エラーメッセージ取得
                        // 「ファイルが存在しません。」
                        errorMessage = ErrorMessagesResources.E9999;

                        return Json(new { res = "NG", error = errorMessage });
                    }
                }
                catch (Exception ex)
                {
                    // エラーメッセージ取得
                    // 「NASに接続できませんでした。」
                    errorMessage = ErrorMessagesResources.E9999;

                    // log取得
                    var exceptionMessage = ex.Message;
                    return Json(new { res = "NG", error = errorMessage + exceptionMessage });
                }
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;

                // log取得
                var exceptionMessage = ex.Message;
                return Json(new { res = "NG", error = errorMessage + exceptionMessage });
            }

        }

        /// <summary>
        /// 車両IDから識別番号を取得
        /// </summary>
        /// <param name="truckID">車両ID</param>
        /// <returns></returns>
        public IActionResult GetIdentifyNumberFromTruckID(int truckID)
        {
            var identifyNumber = M_TripConnectController.SelectIdentifyNumberByTruckId(truckID);
            return Content(identifyNumber.ToString());
        }

        /// <summary>
        /// 枝連番列を追加し、便IDと枝連番でソートする
        /// </summary>
        /// <param name="dt">追加対象のデータテーブル</param>
        /// <returns></returns>
        public DataTable SortDataTableFromBranchConsecutiveNumber(DataTable dt)
        {
            // テーブルに枝連番列を追加
            dt.Columns.Add("branch_consecutive_number", typeof(int)).SetOrdinal(dt.Columns.IndexOf("trip_branch_number_id") + 1);

            // 各便IDごとに
            int maxTripID = (int)dt.Select("trip_id = MAX(trip_id)", "")[0][0];
            for(int id = 1; id <= maxTripID; id++)
            {
                // 便IDに対応する行が存在するか
                var idRows = dt.Select($"trip_id = {id}", "");
                int countIDRows = idRows.Count();
                if (countIDRows > 0)
                {
                    var countBeforeShiftStartTimeRow = 0;　// 到着予定時間が昼勤開始時間より早い行の数
                    var branchConsecutiveNumber = 1;
                    // 各行ごとに
                    for (int i = 0; i < countIDRows; i++)
                    {
                        var dayShiftStartTime = DateTime.Parse(idRows[i]["day_shift_start_time"].ToString());
                        var arrivalScheduledTime = DateTime.Parse(idRows[i]["arrival_scheduled_time"].ToString());

                        // 到着予定時間が昼勤開始時間以降のデータの場合、枝連番付与
                        // それ以外の場合、昼勤開始時間以前の行数のカウントを1増やす
                        if (arrivalScheduledTime > dayShiftStartTime)
                        {
                            DataRow dr = idRows[i];
                            dr["branch_consecutive_number"] = branchConsecutiveNumber;

                            branchConsecutiveNumber++;
                        }
                        else
                        {
                            countBeforeShiftStartTimeRow++;
                        }
                    }

                    // 到着予定時間が昼勤開始時間以前のデータに枝連番付与
                    if (countBeforeShiftStartTimeRow > 0)
                    {
                        for(int i=0; i < countBeforeShiftStartTimeRow; i++)
                        {
                            DataRow dr = idRows[i];
                            dr["branch_consecutive_number"] = branchConsecutiveNumber;
                            branchConsecutiveNumber++;
                        }
                    }
                }
            }
            // 便IDと枝連番でソート
            DataView dv = new DataView(dt);
            dv.Sort = "trip_id, branch_consecutive_number";
            dt = dv.ToTable();
            return dt;
        }
    }
}
