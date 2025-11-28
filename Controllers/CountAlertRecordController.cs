using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Controllers
{
    public class CountAlertRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new AlertRecordViewModel();
            try
            {
                var alertRecordController = new AlertRecordController();
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // アラート履歴情報取得SQL作成
                var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord();
                // DB接続
                List<AlertRecordModel> alertRecordList = AlertRecordConnectController.ConnectTAlertRecords<AlertRecordModel>(alertRecordSql);

                // 便実績情報取得SQL作成
                var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                List<LoadRecordModel> loadRecordList = AlertRecordConnectController.ConnectTAlertRecords<LoadRecordModel>(loadRecordSql);

              
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
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
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
                        var notificationSql = "";
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
                                <td>{alertCount.TripName}_{alertCount.TripBranchSeq}</td>
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
                    TripName = tripName,
                    TripBranchSeq = tripBranchSeq,                    
                };
                alertCounts.Add(countAlertRecordModel);
            }
            return alertCounts;
        }

        private int ContainCount(int count, List<string> alertItems, string compareString)
        {
            if (alertItems.Contains(compareString))
                return count++;
            else return count;
        }
    }
}
