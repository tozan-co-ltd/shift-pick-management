using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class M_TripBranchNumberController : BaseController
    {
        public IActionResult Index(int tripId, string? tripName, bool? isChecked)
        {
            var today = DateTime.Now;
            M_TripBranchNumberModel model = new();
            bool isBeforeApplicablePeriod = true;

            if (isChecked != null)
            {
                model.IsCheckedBeforeApplicablePeriod = isChecked.Value;
                isBeforeApplicablePeriod = isChecked.Value;
            }

            // デバッグ用
            if (string.IsNullOrEmpty(tripName))
            {
                tripId = 3;
                tripName = "豊鉄8t常傭便1";
            }

            model.TripID = tripId;
            model.TripName = tripName;

            try
            {
                // 便マスター情報取得SQL作成
                var sql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(tripId, isBeforeApplicablePeriod, today);
                // DB接続
                IEnumerable<M_TripBranchNumberModel> tripList = M_TripBranchNumberConnectController.ConnectMTripBranchNumbers(sql);

                model.M_TripList = tripList.ToPagedList();

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
        /// 便情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(int tripId, bool isBeforeApplicablePeriod, DateTime applicablePeriod )
        {
            var searchData = string.Empty;
            try
            {
                // 便マスター情報取得SQL作成
                var sql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(tripId, isBeforeApplicablePeriod, applicablePeriod);
                // DB接続
                IEnumerable<M_TripBranchNumberModel> tripList = M_TripBranchNumberConnectController.ConnectMTripBranchNumbers(sql);

                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th width=""40""></th>
                                    <th class=""font-weight-bold"">便枝番ID</th>
                                    <th class=""font-weight-bold"">到着予定時間</th>
                                    <th class=""font-weight-bold"">出発予定時間</th>
                                    <th class=""font-weight-bold"">適用開始日時</th>
                                    <th class=""font-weight-bold"">適用終了日時</th>
                                    <th class=""font-weight-bold"">更新日時</th>
                                    <th class=""font-weight-bold"">更新者</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // 新しい便情報テーブルのhtml作成
                if (tripList.Count() > 0)
                {
                    foreach (var item in tripList)
                    {
                        searchData += $@"
                            <tr>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnEditClick('{@item.TripName}')"" data-id=""@item.TripHistoryID"" data-toggle=""modal"" data-target=""#edit-modal"">
                                        <i class=""fa-solid fa-pen""></i>
                                    </a>
                                </td>
                                <td>{@item.TripBranchNumberID}</td>
                                <td>{@item.ArrivalScheduledTime.ToString("HH:mm")}</td>
                                <td>{@item.DepartureScheduledTime.ToString("HH:mm")}</td>
                                <td>{@item.ApplicableStartDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.ApplicableEndDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.UpdatedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.UpdatedBy}</td>
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
