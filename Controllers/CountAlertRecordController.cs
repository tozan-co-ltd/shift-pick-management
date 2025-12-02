using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace ai_truck_load_measurement.Controllers
{
    public class CountAlertRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new AlertRecordViewModel();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // ログインユーザーのメインデポ情報取得
                model.MainDepoID = user.MainDepoID;
                model.MainDepoName = user.MainDepoName;
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
        /// アラート履歴情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos, List<SelectedTripModel> selectedTrips)
        {
            var searchData = string.Empty;
            List<AlertRecordModel> alertRecordList = new();
            List<LoadRecordModel> loadRecordList = new();
            List<CountAlertRecordModel> countAlertRecordList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    // アラート履歴情報取得SQL作成
                    var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord(startOfPeriod, endOfPeriod, checkedDepos);
                    // DB接続
                    alertRecordList = AlertRecordConnectController.ConnectTAlertRecords<AlertRecordModel>(alertRecordSql);

                    if (alertRecordList.Count > 0)
                    {
                        // アラート履歴に対応する便実績情報取得SQL作成
                        var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                        // DB接続
                        loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                        var notificationSql = CountAlertRecordConnectController.CreateSQLToSelectNotifications(startOfPeriod, endOfPeriod, selectedTrips);
                        var notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(notificationSql);
                        countAlertRecordList = GetAlertCounts(alertRecordList, loadRecordList, notificationList);
                    }
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">便名称_便枝番</th>
                                    <th class=""font-weight-bold"">到着時間(早)</th>
                                    <th class=""font-weight-bold"">出発時間(早)</th>
                                    <th class=""font-weight-bold"">到着時間(遅)</th>
                                    <th class=""font-weight-bold"">出発時間(遅)</th>
                                    <th class=""font-weight-bold"">到着荷量(下限)</th>
                                    <th class=""font-weight-bold"">出発荷量(下限)</th>
                                    <th class=""font-weight-bold"">実績総数</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // テーブルのbody部分
                if (countAlertRecordList.Count > 0)
                {
                    foreach (var alertCount in countAlertRecordList)
                    {

                        searchData += $@"
                            <tr>
                                <td>{alertCount.SelectedTripName}</td>
                                <td>{alertCount.EarlyArriveCount}</td>
                                <td>{alertCount.EarlyDepartCount}</td>
                                <td>{alertCount.LateArriveCount}</td>
                                <td>{alertCount.LateDepartCount}</td>
                                <td>{alertCount.ArrivalLoadCount}</td>
                                <td>{alertCount.DepartureLoadCount}</td>
                                <td>{alertCount.AllRecordCount}</td>
                            </tr>
                    ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

                return Json(new SearchedTripRecordListModel
                {
                    searchedTripRecordHTML = searchData,
                    searchedTripRecordLength = alertRecordList.Count()
                });
            }
            catch (SqlException)
            {
                return Json(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Json(errorMessage);
            }
        }


        public List<CountAlertRecordModel> GetAlertCounts(List<AlertRecordModel> alertRecordList, List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList)
        {
            var alertCounts = new List<CountAlertRecordModel>();
            var earlyArriveCount = 0;
            var lateArriveCount = 0;
            var earlyDepartCount = 0;
            var lateDepartCount = 0;
            var arrivalLoadCount = 0;
            var departureLoadCount = 0;
            var allRecordCount = 0;
            var tripName = "";
            var tripBranchSeq = "";

            foreach (var notification in notificationList)
            {
                var sameNotificationRecords = alertRecordList.FindAll(x => x.NotificationID == notification.NotificationID);
                foreach(var alertRecord in sameNotificationRecords)
                {
                    var loadRecord = loadRecordList.Find(x => x.TripRecordID == alertRecord.TripRecordID);
                    var alertItems = new AlertRecordController().GetAlertItems(alertRecord, loadRecord);
                    earlyArriveCount = ContainCount(earlyArriveCount, alertItems, "到着時間(早)");
                    lateArriveCount = ContainCount(lateArriveCount, alertItems, "到着時間(遅)");
                    earlyDepartCount = ContainCount(earlyDepartCount, alertItems, "出発時間(早)");
                    lateDepartCount = ContainCount(lateDepartCount, alertItems, "出発時間(遅)");
                    arrivalLoadCount = ContainCount(arrivalLoadCount, alertItems, "到着荷量(下限)");
                    departureLoadCount = ContainCount(departureLoadCount, alertItems, "出発荷量(下限)");
                    allRecordCount++;
                    tripName = loadRecord.TripName;
                    tripBranchSeq = loadRecord.TripBranchSeq;
                }
                var countAlertRecordModel = new CountAlertRecordModel()
                {
                    EarlyArriveCount = earlyArriveCount,
                    LateArriveCount = lateArriveCount,
                    EarlyDepartCount = earlyDepartCount,
                    LateDepartCount = lateDepartCount,
                    ArrivalLoadCount = arrivalLoadCount,
                    DepartureLoadCount = departureLoadCount,
                    AllRecordCount = allRecordCount,
                    SelectedTripName = tripName + "_" + tripBranchSeq,
                };
                alertCounts.Add(countAlertRecordModel);
            }
            return alertCounts;
        }

        /// <summary>
        /// 比較対象の文字列がList内に存在する場合、引数をインクリメントして返す
        /// 存在しない場合は引数をそのまま返す
        /// </summary>
        /// <param name="count"></param>
        /// <param name="alertItems">アラート項目</param>
        /// <param name="compareString">比較対象文字列</param>
        /// <returns></returns>
        private int ContainCount(int count, List<string> alertItems, string compareString)
        {
            if (alertItems.Contains(compareString))
                count++;
            return count;
        }

        /// <summary>
        /// 便枝番セレクトリストのHTML取得
        /// </summary>
        /// <param name="startOfPeriod">便の期間開始日</param>
        /// <param name="endOfPeriod">便の期間終了日</param>
        /// <param name="checkedDepos">選択されたデポ</param>
        /// <returns></returns>
        public string GetTripNameAndBranchSeqHTML(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var tripRecordSql = CountAlertRecordConnectController.CreateSQLToSelectTripNameFromPeriodAndNotifications(startOfPeriod, endOfPeriod, checkedDepos);
            var tripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(tripRecordSql);
            var html = LoadRecordController.CreateSelectTripNameAndBranchSeqHTML(tripRecordList, checkedDepos);
            return html;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos, List<SelectedTripModel> selectedTrips)
        {
            string? errorMessage;
            string startDate = startOfPeriod.ToString("yyyyMMdd");
            string endDate = endOfPeriod.ToString("yyyyMMdd");
            try
            {
                // 検索条件シート用データテーブル作成
                DataTable searchConditionDT = new DataTable();
                searchConditionDT.Columns.Add("項目名");
                searchConditionDT.Columns.Add("検索条件");
                // デポの設定
                var selectedDeposName = LoadRecordController.SelectedDepos(checkedDepos);
                searchConditionDT.Rows.Add("対象デポ", selectedDeposName);
                // 稼働日の設定
                searchConditionDT.Rows.Add("稼働日", $"{startDate}～{endDate}");

                var nonTripNameRecordList = new List<NonTripNameRecordModel>();

                var searchData = string.Empty;
                List<AlertRecordModel> alertRecordList = new();
                List<LoadRecordModel> loadRecordList = new();
                List<CountAlertRecordModel> countAlertRecordList = new();
               
                if (checkedDepos.Count > 0)
                {
                    // アラート履歴情報取得SQL作成
                    var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord(startOfPeriod, endOfPeriod, checkedDepos);
                    // DB接続
                    alertRecordList = AlertRecordConnectController.ConnectTAlertRecords<AlertRecordModel>(alertRecordSql);

                    if (alertRecordList.Count > 0)
                    {
                        // アラート履歴に対応する便実績情報取得SQL作成
                        var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                        // DB接続
                        loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                        var notificationSql = CountAlertRecordConnectController.CreateSQLToSelectNotifications(startOfPeriod, endOfPeriod, selectedTrips);
                        var notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(notificationSql);
                        countAlertRecordList = GetAlertCounts(alertRecordList, loadRecordList, notificationList);
                    }
                }

                var countAlertRecordDT = Utils.ToDataTable<CountAlertRecordModel>(countAlertRecordList);

                // 便実績が0の場合
                if (countAlertRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"アラート回数詳細_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "アラート回数詳細";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(countAlertRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, checkedDepos);

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
    }
}
