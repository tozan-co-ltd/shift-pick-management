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
            List<AlertRecordModel> alertRecordList = new();
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
                if (alertRecordList.Count > 0)
                {
                    foreach (var alertRecord in alertRecordList)
                    {
                        // アラート履歴に対応した便実績
                        var loadRecord = loadRecordList.Find(x => x.TripRecordID == alertRecord.TripRecordID)!;
                        // アラート項目部分のhtml取得
                        var alertItems = GetAlertItems(alertRecord, loadRecord);
                        var alertItemsHTML = ConversionAlertItemsToHTML(alertItems);

                        searchData += $@"
                            <tr>
                                <td hidden>{alertRecord.AlertRecordID}</td>
                                <td>{loadRecord.TripName}</td>
                                <td>{loadRecord.TripBranchSeq}</td>
                                <td>{loadRecord.StationName}</td>
                                <td>{loadRecord.DriverName}</td>
                                <td>{alertItemsHTML}</td>
                                <td>{loadRecord.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalAlertImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnDepartureAlertImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                    <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
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

    }
}
