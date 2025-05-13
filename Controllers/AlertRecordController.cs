using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Controllers
{
    public class AlertRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new AlertRecordViewModel();
            // ログインユーザーのメインデポ情報取得
            model.MainDepo = GetMainDepo();
            return View(model);
        }

        /// <summary>
        /// 便情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<AlertRecordModel> alertRecordList = new();
            List<LoadRecordModel> loadRecordList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    // アラート履歴情報取得SQL作成
                    var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord(startOfPeriod, endOfPeriod);
                    // DB接続
                    alertRecordList = AlertRecordConnectController.ConnectTAlertRecords(alertRecordSql);
                    // アラート履歴に対応する便実績情報取得SQL作成
                    var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                    // DB接続
                    loadRecordList = LoadRecordConnectController.ConnectTTripRecords(loadRecordSql);
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">便枝番</th>
                                    <th class=""font-weight-bold"">ステーション</ br>名</th>
                                    <th class=""font-weight-bold"">乗務員</th>
                                    <th class=""font-weight-bold"">アラート項目</th>
                                    <th class=""font-weight-bold"">稼働日</th>
                                    <th class=""font-weight-bold"">到着情報</th>
                                    <th class=""font-weight-bold"">出発情報</th>
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
                        var alertItems = GetAlertItemsHTML(alertRecord, loadRecord);

                        searchData += $@"
                            <tr>
                                <td>{loadRecord.TripName}</td>
                                <td>{loadRecord.TripBranchSeq}</td>
                                <td>{loadRecord.StationName}</td>
                                <td>{loadRecord.DriverName}</td>
                                <td>{alertItems}</td>
                                <td>{loadRecord.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalAlertImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnDepartureLoadImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
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
        /// アラート項目のHTML取得
        /// </summary>
        /// <param name="alertRecord"></param>
        /// <param name="loadRecord"></param>
        /// <returns></returns>
        public string GetAlertItemsHTML(AlertRecordModel alertRecord, LoadRecordModel loadRecord)
        {
            var alertItems = new List<string>();

            // 到着時間判定
            var arrivalTimeAlert = TimeAlertDetermination(loadRecord!.ArrivalScheduledTime, loadRecord.ArrivedAt);
            if (!string.IsNullOrEmpty(arrivalTimeAlert))
                alertItems.Add("到着時間" + arrivalTimeAlert);

            // 出発時間判定
            var departureTimeAlert = TimeAlertDetermination(loadRecord!.DepartureScheduledTime, loadRecord.DepartedAt);
            if(!string.IsNullOrEmpty(departureTimeAlert))
                alertItems.Add("出発時間" + departureTimeAlert);

            // 到着荷量判定
            if (loadRecord.ArrivalLoadClass != -1 && loadRecord.ArrivalLoadClass < alertRecord.ArrivalLowerLoadClass)
                alertItems.Add("到着荷量(下限)");

            // 出発荷量判定
            if (loadRecord.DepartureLoadClass != -1 && loadRecord.DepartureLoadClass < alertRecord.DepartureLowerLoadClass)
                alertItems.Add("出発荷量(下限)");

            // html作成
            var html = CreateAlertHTML(alertItems);

            return html;
        }

        /// <summary>
        /// 時間アラート判定
        /// </summary>
        /// <param name="scheduledTime"></param>
        /// <param name="recordtime"></param>
        /// <returns></returns>
        private string TimeAlertDetermination(DateTime scheduledTime, DateTime recordtime)
        {
            var timeAlert = "";
            var scheduledTimeToday = new DateTime(recordtime.Year, recordtime.Month, recordtime.Day, scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            if (scheduledTimeToday.AddHours(-1) > recordtime)
                timeAlert = "(早)";
            else if (recordtime > scheduledTimeToday.AddHours(1))
                timeAlert = "(遅)";

            return timeAlert;
        }

        /// <summary>
        /// アラート項目リストからhtmlを作成する
        /// </summary>
        /// <param name="alertItems">アラート項目リスト</param>
        /// <returns></returns>
        public string CreateAlertHTML(List<string> alertItems)
        {
            var html = "";
            for(int i = 0; i<alertItems.Count; i++)
            {
                if (i > 0)
                    html += "<br>";
                html += alertItems[i];
            }
            return html;
        }
    }
}
