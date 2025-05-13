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
                    // 便マスター情報取得SQL作成
                    var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord(startOfPeriod, endOfPeriod);
                    // DB接続
                    alertRecordList = AlertRecordConnectController.ConnectTAlertRecords(alertRecordSql);
                    var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                    loadRecordList = LoadRecordConnectController.ConnectTTripRecords(loadRecordSql);
                }

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
                // 新しい便情報テーブルのhtml作成
                if (alertRecordList.Count > 0)
                {
                    foreach (var item in alertRecordList)
                    {
                        searchData += $@"
                            <tr>
                                <td>{@item.TripName}</td>
                                <td>{@item.TripBranchSeq}</td>
                                <td>{@item.StationName}</td>
                                <td>{@item.DriverName}</td>
                                <td>アラート項目<br>仮置き</td>
                                <td>{@item.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalAlertImageClick('{item.AlertRecordID}', this)"" data-id=""{item.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnDepartureLoadImageClick('{item.AlertRecordID}', this)"" data-id=""{item.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
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
    }
}
