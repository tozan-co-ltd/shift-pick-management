using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Controllers
{
    public class ArrivalTimeDefferenceController : BaseController
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
        /// アラート履歴情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> trips)
        {
            var searchData = string.Empty;
            List<LoadRecordModel> loadRecordList = new();
            try
            {
                if (trips.Count > 0)
                {
                    // アラート履歴に対応する便実績情報取得SQL作成
                    var loadRecordSql =ArrivalTimeDefferenceConnectController.CreateSQLToSelectArrivalRecordAndSchedule(trips, startOfPeriod, endOfPeriod);
                    // DB接続
                    loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                }

                // テーブルのヘッダ部分
                searchData += GetTableHeader(trips);

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
                                var comparisonTime = new DateTime(1900, 1, 1, targetDateRecord.ArrivedAt.Hour, targetDateRecord.ArrivedAt.Minute, targetDateRecord.ArrivedAt.Second);
                                var timeDeff = GetTimeDeff(targetDateRecord.ArrivalScheduledTime, comparisonTime);

                                var timeDeffString = "";
                                if (timeDeff < 0) timeDeffString = $"{timeDeff}m";
                                else timeDeffString = $"+{timeDeff}m";
                                    searchData += $@"
                                <td>{timeDeffString}</td>
                            ";
                            }
                            else
                            {
                                searchData += $@"
                                <td>-</td>
                            ";
                            }
                        }

                        searchData += $@"
                            </tr>
                        ";
                    }

                    searchData += GetAverageTimeDeff(loadRecordList, trips);
                    searchData += GetCountEarlyTimeOver(loadRecordList, trips);
                    searchData += GetCountLateTimeOver(loadRecordList, trips);
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

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


        private string GetTableHeader( List<SelectedTripModel> trips)
        {
            var tableHeader = $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold""></th>
            ";

            foreach (var trip in trips)
            {
                tableHeader += $@"<th class=""font-weight-bold"">{trip.TripName}_{trip.TripBranchSeq}</th>
                ";
            }

            tableHeader += $@"
                                </tr>
                            </thead>
                            <tbody>
            ";
            return tableHeader;
        }

        private int GetTimeDeff(DateTime targetDate, DateTime comparisonTime)
        {
            var timeDeff = (comparisonTime - targetDate).TotalMinutes;
            if (timeDeff >= 60 * 20)
                timeDeff -= 60 * 24;
            else if (timeDeff <= -60 * 23)
                timeDeff += 60 * 24;
            return (int)timeDeff;
        }

        public string GetAverageTimeDeff(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var averageTimeDeffString = $@"
                    <tr>
                        <td>平均ズレ時間</td>

            ";
            foreach(var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 予実差の総和を取得
                var targetTripTimeDeffSam = 0;
                foreach (var tripRecord in targetTripRecords)
                {
                    var comparisonTime = new DateTime(1900, 1, 1, tripRecord.ArrivedAt.Hour, tripRecord.ArrivedAt.Minute, tripRecord.ArrivedAt.Second);
                    targetTripTimeDeffSam += GetTimeDeff(tripRecord.ArrivalScheduledTime, comparisonTime);
                }

                // 便毎の予実差の平均取得
                var averageTimeDeff = Math.Ceiling((double)targetTripTimeDeffSam / (double)targetTripRecords.Count * 10) /10;

                averageTimeDeffString += $@"
                        <td>{averageTimeDeff}m</td>
                ";
            }
            averageTimeDeffString += "</tr>";

            return averageTimeDeffString;
        }

        private string GetCountEarlyTimeOver(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var earlyTimeOverString = $@"
                    <tr>
                        <td>アラート範囲(早着)</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 予実差の総和を取得
                var targetTripEarlyOverCount = 0;
                foreach (var tripRecord in targetTripRecords)
                {
                    var comparisonTime = new DateTime(1900, 1, 1, tripRecord.ArrivedAt.Hour, tripRecord.ArrivedAt.Minute, tripRecord.ArrivedAt.Second);
                    var timeDeff = GetTimeDeff(tripRecord.ArrivalScheduledTime, comparisonTime);
                    if(timeDeff <= -60)
                        targetTripEarlyOverCount++;
                }


                earlyTimeOverString += $@"
                        <td>{targetTripEarlyOverCount}回</td>
                ";
            }
            earlyTimeOverString += "</tr>";

            return earlyTimeOverString;
        }

        private string GetCountLateTimeOver(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var lateTimeOverString = $@"
                    <tr>
                        <td>アラート範囲(遅着)</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 予実差の総和を取得
                var targetTripLateOverCount = 0;
                foreach (var tripRecord in targetTripRecords)
                {
                    var comparisonTime = new DateTime(1900, 1, 1, tripRecord.ArrivedAt.Hour, tripRecord.ArrivedAt.Minute, tripRecord.ArrivedAt.Second);
                    var timeDeff = GetTimeDeff(tripRecord.ArrivalScheduledTime, comparisonTime);
                    if (timeDeff >= +20)
                        targetTripLateOverCount++;
                }


                lateTimeOverString += $@"
                        <td>{targetTripLateOverCount}回</td>
                ";
            }
            lateTimeOverString += "</tr>";

            return lateTimeOverString;
        }
    }
}
