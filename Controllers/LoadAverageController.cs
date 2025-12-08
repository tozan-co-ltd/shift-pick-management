using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

namespace ai_truck_load_measurement.Controllers
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
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> trips)
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

                        searchData += $@"
                            <tr>
                                <td>{date.ToString("yyyy/MM/dd")}</td>
                        ";

                        foreach (var trip in trips)
                        {
                            var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                            if (targetDateRecord != null)
                            {
                                var arrivalLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(targetDateRecord.ArrivalLoadClass);
                                var departureLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(targetDateRecord.DepartureLoadClass);
                                searchData += $@"
                                <td>{arrivalLoadStatus}</td>
                                <td>{departureLoadStatus}</td>
                                ";
                            }
                            else
                            {
                                searchData += $@"
                                <td>-</td>
                                <td>-</td>
                            ";
                            }
                        }

                        searchData += $@"
                            </tr>
                        ";
                    }

                    searchData += $@"
                            </tbody>
                            <tbody>
                            <tr hidden></tr>
                            {GetAverageLoadStatusesHTML(loadRecordList, trips)}
                            {GetLowerLoadStatusesHTML(notificationList, trips)}
                            {GetCountLowerLoadAlertHTML(loadRecordList, notificationList, trips)}
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
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center arrival-time-deff-table"" id=""{tableID}"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">稼働日</th>
            ";

            foreach (var trip in trips)
            {
                tableHeader += $@"
                                    <th class=""font-weight-bold"">{trip.TripName}_{trip.TripBranchSeq} 到着</th>
                                    <th class=""font-weight-bold"">{trip.TripName}_{trip.TripBranchSeq} 出発</th>
                ";
            }

            tableHeader += $@"
                                </tr>
                            </thead>
                            <tbody>
            ";
            return tableHeader;
        }

        /// <summary>
        /// 荷量平均HTML取得
        /// </summary>
        /// <param name="loadRecordList">実績リスト</param>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetAverageLoadStatusesHTML(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var averageLoadStatusesString = $@"
                    <tr class=""arrival-time-deff-table-top mt-2"" >
                        <td>荷量平均</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 便毎の到着・出発荷量の平均取得
                averageLoadStatusesString += $@"
                        <td>{GetAverageLoadStatuse(targetTripRecords, true)}</td>
                        <td>{GetAverageLoadStatuse(targetTripRecords, false)}</td>
                ";
            }
            averageLoadStatusesString += "</tr>";

            return averageLoadStatusesString;
        }

        /// <summary>
        /// 荷量平均取得
        /// </summary>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="isArrive">到着か否か</param>
        /// <returns></returns>
        private string GetAverageLoadStatuse(List<LoadRecordModel> loadRecordList, bool isArrive)
        {
            var totalLoadStatusMax = 0.0;
            var totalLoadStatusMin = 0.0;
            var loadRecordCount = 0;
            var loadClass = 0;
            foreach (var loadRecord in loadRecordList)
            {
                if (isArrive)
                    loadClass = loadRecord.ArrivalLoadClass;
                else
                    loadClass = loadRecord.DepartureLoadClass;

                if (loadClass == 2)
                    loadRecordCount++;

                if (loadClass >= 3)
                {
                    // 荷量クラスから荷量の値の最低値、最高値に変換
                    totalLoadStatusMin += (loadClass - 3) * 10 + 1;
                    totalLoadStatusMax += (loadClass - 2) * 10;
                    loadRecordCount++;
                }
            }

            return GetAverageLoadStatusString(loadRecordCount, totalLoadStatusMin, totalLoadStatusMax);
        }

        private string GetAverageLoadStatusString(int loadRecordCount, double totalLoadStatusMin, double totalLoadStatusMax)
        {
            if (loadRecordCount <= 0)
                return "-";

            return
                totalLoadStatusMax != 0
                        ? $"{Math.Round(totalLoadStatusMin / loadRecordCount, MidpointRounding.AwayFromZero)}-{Math.Round(totalLoadStatusMax / loadRecordCount, MidpointRounding.AwayFromZero)}%"
                        : "0%";
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
                    <tr style=""font-weight: bold; font-style:italic"" >
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
        private string GetCountLowerLoadAlertHTML(List<LoadRecordModel> loadRecordList, List<M_NotificationModel> notificationList, List<SelectedTripModel> trips)
        {
            var lowerLoadStatusesHTML = $@"
                    <tr class=""arrival-time-deff-table-bottom mt-2"" >
                        <td>荷量アラート回数</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                // 便毎に対応する通知マスターを取得
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if (targetTripRecords != null && targetNotification != null)
                    // 便名称_便枝番に対応する実績と通知マスターが両方存在する場合
                    // HTMLに到着、出発の荷量下限を下回った回数をそれぞれ記入
                    lowerLoadStatusesHTML += $@"
                            <td>{targetTripRecords.FindAll(x => x.ArrivalLoadClass < targetNotification.ArrivalLowerLoadClass).Count}回</td>
                            <td>{targetTripRecords.FindAll(x => x.DepartureLoadClass < targetNotification.DepartureLowerLoadClass).Count}回</td>
                    ";
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
            // 平均荷量行追加
            convertedTable.Rows.Add(GetAverageLoadStatusesRow(convertedTable, loadRecordList, trips));
            // 荷量下限値行追加
            convertedTable.Rows.Add(GetLowerLoadStatusesRow(convertedTable, loadRecordList, notificationList, trips));
            // 荷量アラート判定回数行追加
            convertedTable.Rows.Add(GetCountLowerLoadAlertRow(convertedTable, loadRecordList, notificationList, trips));
            return convertedTable;
        }

        /// <summary>
        /// 平均荷量行追加
        /// </summary>
        /// <param name="convertedTable">追加先テーブル</param>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        private DataRow GetAverageLoadStatusesRow(DataTable convertedTable, List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var dataRow = convertedTable.NewRow();
            dataRow["workDay"] = "平均ズレ時間";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 便毎の到着・出発荷量の平均取得
                dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = GetAverageLoadStatuse(loadRecordList, true);
                dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = GetAverageLoadStatuse(loadRecordList, false);
            }
            return dataRow;
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
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 到着"] = targetTripRecords.FindAll(x => x.ArrivalLoadClass < targetNotification.ArrivalLowerLoadClass).Count + "回";
                    dataRow[trip.TripName + "_" + trip.TripBranchSeq + " 出発"] = targetTripRecords.FindAll(x => x.DepartureLoadClass < targetNotification.DepartureLowerLoadClass).Count + "回";
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
