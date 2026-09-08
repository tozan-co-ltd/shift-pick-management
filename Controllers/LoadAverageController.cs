using shift_pick_management.Commons;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

namespace shift_pick_management.Controllers
{
    public class LoadAverageController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadRecordViewModel();
            var user = ClaimsLoginUserData();
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;
            return View(model);
        }


        /// <summary>
        /// 荷量平均表示テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> trips, bool isRecordCountOneOrMore)
        {
            var searchData = string.Empty;
            List<LoadRecordModel> loadRecordList = new();
            List<M_NotificationModel> notificationList = new();
            try
            {
                if (trips.Count > 0)
                {
                    // 選択した便_便枝番に対応する便実績情報取得SQL作成
                    var loadRecordSql = LoadAverageConnectController.CreateSQLToSelectLoadClasses(trips, startOfPeriod, endOfPeriod);
                    // DB接続
                    loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                    // 選択した便_便枝番に対応する通知マスター情報取得SQL作成
                    var notificationSQL = LoadAverageConnectController.CreateSQLToSelectNotificationsFromTrips(trips, startOfPeriod, endOfPeriod);
                    // DB接続
                    notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(notificationSQL);
                    foreach (M_NotificationModel notification in notificationList)
                    {
                        // 荷量クラスを％表示に変換
                        notification.ArrivalLowerLoadStatus = M_NotificationController.ConversionLoadClassToLoadStatus(notification.ArrivalLowerLoadClass);
                        notification.DepartureLowerLoadStatus = M_NotificationController.ConversionLoadClassToLoadStatus(notification.DepartureLowerLoadClass);
                    }

                }

                // テーブルのヘッダ部分
                searchData += GetTableHeader(trips, "tripTable");

                // テーブルのbody部分
                if (loadRecordList.Count > 0)
                {
                    for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
                    {
                        var targetDateRecords = loadRecordList.FindAll(x => x.WorkDay == date);

                        // 選択した日付に実績が登録されていない、かつそれらを表示しない場合、処理を飛ばす
                        if (targetDateRecords.Count == 0 && isRecordCountOneOrMore)
                            continue;

                        searchData += $@"
                            <tr>
                                <td>{date.ToString("yyyy/MM/dd")}</td>
                        ";

                        foreach (var trip in trips)
                        {
                            var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                            var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);
                            searchData += GetLoadStatusesHTML(targetDateRecord, trip, targetNotification);
                        }

                        searchData += $@"
                            </tr>
                        ";
                    }

                    searchData += $@"
                            </tbody>
                            <tbody>
                            <tr hidden></tr>
                            {GetLowerLoadStatusesHTML(notificationList, trips)}
                            {GetCountLowerLoadAlertsHTML(loadRecordList, notificationList, trips)}
                            </tbody>
                    </div>
                    ";
                }


                return Json(new SearchedTripRecordListModel
                {
                    searchedTripRecordHTML = searchData,
                    searchedTripRecordLength = loadRecordList.Count()
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

        /// <summary>
        /// 選択した便毎にデータテーブルのヘッダー箇所を作成する
        /// </summary>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetTableHeader(List<SelectedTripModel> trips, string tableID)
        {
            var tableHeader = $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center statistics-table"" id=""{tableID}"">
                            <thead>
                                <tr align=""center"">
                                    <th rowspan=""2"" style=""vertical-align: middle; border-left: 1px solid #404040;"">稼働日</th>
            ";

            foreach(var trip in trips)
                tableHeader += $@"
                                    <th class=""font-weight-bold"" colspan=""2"">{trip.TripName}_{trip.TripBranchSeq}</th>
                ";

            tableHeader += $@"
                                </tr>
                                <tr align=""center"">
            ";

            foreach (var trip in trips)
                tableHeader += $@"
                                    <th class=""font-weight-bold"" style=""border-left: 0;"">到着</th>
                                    <th class=""font-weight-bold"">出発</th>
                ";

            tableHeader += $@"
                                </tr>
                            </thead>
                            <tbody>
            ";
            return tableHeader;
        }

        private string GetLoadStatusesHTML(LoadRecordModel? loadRecord, SelectedTripModel trip, M_NotificationModel? notification)
        {
            var arrivalLowerLoadClass = 0;
            var departureLowerLoadClass = 0;

            if (notification != null)
            {
                arrivalLowerLoadClass = notification.ArrivalLowerLoadClass;
                departureLowerLoadClass = notification.DepartureLowerLoadClass;
            }

            if (loadRecord != null)
                return $@"      
                                {GetLoadStatusHTML(loadRecord.ArrivalLoadClass, arrivalLowerLoadClass)}
                                {GetLoadStatusHTML(loadRecord.DepartureLoadClass, departureLowerLoadClass)}
                ";
            else
                return $@"
                                <td>-</td>
                                <td>-</td>
                ";
        }

        private string GetLoadStatusHTML(int loadClass, int lowerLoadClass)
        {
            var loadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(loadClass);
            var emphasizedColorStyle = "";
            if (loadClass < lowerLoadClass && loadClass != -1)
                emphasizedColorStyle = $" style=\"background-color: #FFE5E5;\"";
            var html = $"<td{emphasizedColorStyle}>{loadStatus}</td>";
            return html;
        }

        /// <summary>
        /// 荷量下限HTML取得
        /// </summary>
        /// <param name="notificationList">通知マスターリスト</param>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetLowerLoadStatusesHTML(List<M_NotificationModel> notificationList, List<SelectedTripModel> trips)
        {
            var lowerLoadStatusesHTML = $@"
                    <tr  class=""statistics-table-top mt-2"" >
                        <td>荷量下限値</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に対応する通知マスターを取得
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if(targetNotification != null)
                    // 対応する通知マスターが存在する場合
                    // 荷量下限を表示
                    lowerLoadStatusesHTML += $@"
                            <td>{targetNotification.ArrivalLowerLoadStatus}</td>
                            <td>{targetNotification.DepartureLowerLoadStatus}</td>
                    ";
                else
                    lowerLoadStatusesHTML += $@"
                            <td>設定無し</td>
                            <td>設定無し</td>
                    ";
            }
            lowerLoadStatusesHTML += "</tr>";

            return lowerLoadStatusesHTML;
        }

        /// <summary>
        /// 荷量アラート回数HTML取得
        /// </summary>
        /// <param name="loadRecordList">実績リスト</param>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetCountLowerLoadAlertsHTML(List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList, List<SelectedTripModel> trips)
        {
            var lowerLoadStatusesHTML = $@"
                    <tr class=""statistics-table-bottom mt-2"" >
                        <td>荷量アラート回数</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                // 便毎に対応する通知マスターを取得
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if (targetTripRecords != null && targetNotification != null)
                {
                    // 便名称_便枝番に対応する実績と通知マスターが両方存在する場合
                    // HTMLに到着、出発の荷量下限を下回った回数をそれぞれ記入
                    // 通知マスターに下限荷量が設定されていない場合、「設定無し」と記入
                    if (targetNotification.ArrivalLowerLoadClass > 2)
                        lowerLoadStatusesHTML += $@"
                            <td>{targetTripRecords.FindAll(x => x.ArrivalLoadClass < targetNotification.ArrivalLowerLoadClass && x.ArrivalLoadClass >= 2).Count}回</td>
                        ";
                    else
                        lowerLoadStatusesHTML += $@"
                            <td>設定無し</td>
                        ";

                    if (targetNotification.DepartureLowerLoadClass > 2)
                        lowerLoadStatusesHTML += $@"
                            <td>{targetTripRecords.FindAll(x => x.DepartureLoadClass < targetNotification.DepartureLowerLoadClass && x.DepartureLoadClass >= 2).Count}回</td>
                        ";
                    else
                        lowerLoadStatusesHTML += $@"
                            <td>設定無し</td>
                        ";
                }
                else
                        // 存在しない場合、「設定無し」と記入
                        lowerLoadStatusesHTML += $@"
                            <td>設定無し</td>
                            <td>設定無し</td>
                    ";
            }
            lowerLoadStatusesHTML += "</tr>";

            return lowerLoadStatusesHTML;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> arrayTrips, List<string> checkedDepos)
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
                // 期間の設定
                searchConditionDT.Rows.Add("稼働日", $"{startDate}～{endDate}");
                // 選択された便の設定
                var selectedTripNames = "";
                for (int i = 0; i < arrayTrips.Count; i++)
                {
                    if (i != 0)
                    {
                        selectedTripNames += ", ";
                    }
                    selectedTripNames += arrayTrips[i].SelectedTripName;
                }
                searchConditionDT.Rows.Add("選択された便", selectedTripNames);

                // 便実績情報取得
                var tTripRecordSql = LoadAverageConnectController.CreateSQLToSelectLoadClasses(arrayTrips, startOfPeriod, endOfPeriod);
                var tTripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(tTripRecordSql);
                // 選択した便_便枝番に対応する通知マスター情報取得SQL作成
                var notificationSQL = LoadAverageConnectController.CreateSQLToSelectNotificationsFromTrips(arrayTrips, startOfPeriod, endOfPeriod);
                // DB接続
                var notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(notificationSQL);
                foreach (M_NotificationModel notification in notificationList)
                {
                    // 荷量クラスを％表示に変換
                    notification.ArrivalLowerLoadStatus = M_NotificationController.ConversionLoadClassToLoadStatus(notification.ArrivalLowerLoadClass);
                    notification.DepartureLowerLoadStatus = M_NotificationController.ConversionLoadClassToLoadStatus(notification.DepartureLowerLoadClass);
                }
                var tTripRecordDT = ConversionLoadRecordToDataTable(tTripRecordList, notificationList, arrayTrips, startOfPeriod, endOfPeriod);

                // 便実績が0の場合
                if (tTripRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"荷量実績_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "荷量実績";
                string sheetNameTwo = "検索条件";

                // ヘッダ用便名称_便枝番リスト作成
                var selectedTripNameAndBranchSeqs = GetTripNameAndBranchSeqs(arrayTrips);

                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(tTripRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, selectedTripNameAndBranchSeqs);

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
        /// 便実績から予実差平均表示用のデータテーブルに変換する
        /// </summary>
        /// <param name="loadRecordList"></param>
        /// <param name="trips"></param>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <returns></returns>
        public DataTable ConversionLoadRecordToDataTable(List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList, List<SelectedTripModel> trips, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var convertedTable = new DataTable();
            // 列情報設定
            convertedTable.Columns.Add("workDay");
            foreach (var trip in trips)
            {
                convertedTable.Columns.Add(trip.TripName + "_" + trip.TripBranchSeq + " 到着");
                convertedTable.Columns.Add(trip.TripName + "_" + trip.TripBranchSeq + " 出発");
            }
            // 日付ごとに行追加
            for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
            {
                var targetDateRecords = loadRecordList.FindAll(x => x.WorkDay == date);
                var dataRow = convertedTable.NewRow();
                dataRow["workDay"] = date.ToString("yyyy/MM/dd");
                foreach (var trip in trips)
                {
                    var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                    if (targetDateRecord != null)
                    {
                        dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = LoadRecordController.ConversionLoadClassToLoadStatus(targetDateRecord.ArrivalLoadClass);
                        dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = LoadRecordController.ConversionLoadClassToLoadStatus(targetDateRecord.DepartureLoadClass);
                    }
                    else
                    {
                        dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = "-";
                        dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = "-";

                    }
                }
                convertedTable.Rows.Add(dataRow);
            }
            // 荷量下限値行追加
            convertedTable.Rows.Add(GetLowerLoadStatusesRow(convertedTable, loadRecordList, notificationList, trips));
            // 荷量アラート判定回数行追加
            convertedTable.Rows.Add(GetCountLowerLoadAlertRow(convertedTable, loadRecordList, notificationList, trips));
            return convertedTable;
        }

        /// <summary>
        /// 荷量下限行追加
        /// </summary>
        /// <param name="convertedTable">追加先テーブル</param>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        private DataRow GetLowerLoadStatusesRow(DataTable convertedTable, List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList, List<SelectedTripModel> trips)
        {
            var dataRow = convertedTable.NewRow();
            dataRow["workDay"] = "荷量下限値";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if(targetNotification != null)
                {
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = targetNotification.ArrivalLowerLoadStatus;
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = targetNotification.DepartureLowerLoadStatus;
                }
                else
                {
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = "設定無し";
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = "設定無し";
                }
            }
            return dataRow;
        }

        /// <summary>
        /// 荷量アラート判定回数行追加
        /// </summary>
        /// <param name="convertedTable">追加先テーブル</param>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        private DataRow GetCountLowerLoadAlertRow(DataTable convertedTable, List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList, List<SelectedTripModel> trips)
        {
            var dataRow = convertedTable.NewRow();
            dataRow["workDay"] = "荷量アラート判定回数";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 便毎に対応する通知マスターを取得
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if (targetTripRecords != null && targetNotification != null)
                {
                    // 便名称_便枝番に対応する実績と通知マスターが両方存在する場合
                    // HTMLに到着、出発の荷量下限を下回った回数をそれぞれ記入
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = targetTripRecords.FindAll(x => x.ArrivalLoadClass < targetNotification.ArrivalLowerLoadClass && x.ArrivalLoadClass >= 2).Count + "回";
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = targetTripRecords.FindAll(x => x.DepartureLoadClass < targetNotification.DepartureLowerLoadClass && x.DepartureLoadClass >= 2).Count + "回";
                }
                else
                {
                    // 存在しない場合、「設定無し」と記入
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = "設定無し";
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = "設定無し";
                }
            }
            return dataRow;
        }

        /// <summary>
        /// 「便名称_便枝番」のリスト作成
        /// </summary>
        /// <param name="trips"></param>
        /// <returns></returns>
        private List<string> GetTripNameAndBranchSeqs(List<SelectedTripModel> trips)
        {
            var tripNameAndBranchSeqs = new List<string>();
            foreach (var trip in trips)
            {
                var tripNameAndBranchSeq = $"{trip.TripName}_{trip.TripBranchSeq}";
                tripNameAndBranchSeqs.Add(tripNameAndBranchSeq);
            }
            return tripNameAndBranchSeqs;
        }
    }
}
