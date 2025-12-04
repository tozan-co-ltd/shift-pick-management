using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
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
                var averageLoadStatuses = GetAverageLoadStatuses(targetTripRecords);

                averageLoadStatusesString += $@"
                        <td>{averageLoadStatuses[0]}</td>
                        <td>{averageLoadStatuses[1]}</td>
                ";
            }
            averageLoadStatusesString += "</tr>";

            return averageLoadStatusesString;
        }

        /// <summary>
        /// 荷量平均取得
        /// </summary>
        /// <param name="loadRecordList"></param>
        /// <returns></returns>
        private List<string> GetAverageLoadStatuses(List<LoadRecordModel> loadRecordList)
        {
            var averageLoadStatuses = new List<string>();
            var totalArrivalLoadStatusMax = 0.0;
            var totalArrivalLoadStatusMin = 0.0;
            var arrivalLoadRecordCount = 0;
            var totalDepartureLoadStatusMax = 0.0;
            var totalDepartureLoadStatusMin = 0.0;
            var departureLoadRecordCount = 0;
            foreach(var loadRecord in loadRecordList)
            {
                if(loadRecord.ArrivalLoadClass == 2)
                    arrivalLoadRecordCount++;

                if (loadRecord.ArrivalLoadClass >= 3)
                {
                    totalArrivalLoadStatusMin += (loadRecord.ArrivalLoadClass - 3) * 10 + 1;
                    totalArrivalLoadStatusMax += (loadRecord.ArrivalLoadClass - 2) * 10;
                    arrivalLoadRecordCount++;
                }

                if (loadRecord.DepartureLoadClass == 2)
                    departureLoadRecordCount++;

                if(loadRecord.DepartureLoadClass >= 3)
                {
                    totalDepartureLoadStatusMin += (loadRecord.DepartureLoadClass - 3) * 10 + 1;
                    totalDepartureLoadStatusMax += (loadRecord.DepartureLoadClass - 2) * 10;
                    departureLoadRecordCount++;
                }
            }

            averageLoadStatuses.Add(GetAverageLoadStatusString(arrivalLoadRecordCount, totalArrivalLoadStatusMin, totalArrivalLoadStatusMax));
            averageLoadStatuses.Add(GetAverageLoadStatusString(departureLoadRecordCount, totalDepartureLoadStatusMin, totalDepartureLoadStatusMax));

            return averageLoadStatuses;
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
        /// 荷量平均HTML取得
        /// </summary>
        /// <param name="loadRecordList">実績リスト</param>
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
                // 便毎に実績のリストを作成
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if(targetNotification != null)
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
        /// 荷量平均HTML取得
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
                // 便毎に実績のリストを作成
                var targetNotification = notificationList.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq);

                if (targetTripRecords != null && targetNotification != null)
                    lowerLoadStatusesHTML += $@"
                            <td>{targetTripRecords.FindAll(x => x.ArrivalLoadClass < targetNotification.ArrivalLowerLoadClass).Count}回</td>
                            <td>{targetTripRecords.FindAll(x => x.DepartureLoadClass < targetNotification.DepartureLowerLoadClass).Count}回</td>
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

    }
}
