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
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<M_TripBranchNumberModel> trips)
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
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th hidden>便実績ID</th>
                                    <th class=""font-weight-bold""></th>
                ";
                foreach(var trip in trips)
                {
                    searchData += $@"<th class=""font-weight-bold"">{trip.TripName}_{trip.TripBranchSeq}</th>
                    ";
                }
                searchData += $@"
                                </tr>
                            </thead>
                            <tbody>
                ";
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
                            var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName);
                            if (targetDateRecord != null)
                            {
                                var comparisonTime = new DateTime(0, 0, 0, targetDateRecord.ArrivedAt.Hour, targetDateRecord.ArrivedAt.Minute, targetDateRecord.ArrivedAt.Second);
                                var timeDeff = (comparisonTime - targetDateRecord.ArrivalScheduledTime).TotalMinutes;
                                if (timeDeff >= 60 * 20)
                                    timeDeff -= 60 * 24;
                                else if (timeDeff <= - 60 * 23)
                                    timeDeff += 60 * 24;
                                searchData += $@"
                                <td>{(int)timeDeff}</td>
                            ";
                            }
                            else
                            {
                                searchData += $@"
                                <td>なし</td>
                            ";
                            }
                        }

                        searchData += $@"
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

    }
}
