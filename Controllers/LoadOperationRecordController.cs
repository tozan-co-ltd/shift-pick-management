using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadOperationRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadOperationRecordModel();
            var today = DateTime.Now;
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectTripNameFromWorkDays(today);
                // DB接続
                List<SelectListItem> tripNameList = LoadDistributionConnectController.ConnectTTripRecordsForTripName(sql);

                model.TripNameList = tripNameList;

                // 便実績情報取得SQL作成
                var sql2 = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadDistributionModel> tripRecordList = LoadDistributionConnectController.ConnectTTripRecords(sql2);
                // テーブル情報を変換
                tripRecordList = (IEnumerable<LoadDistributionModel>)LoadRecordController.ConversionForTable(tripRecordList);

                model.TripRecordList = tripRecordList.ToPagedList();
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
            /// 指定した期間内に存在する便名称のリストを取得してセレクトリストアイテム化する
            /// </summary>
            /// <param name="startOfPeriod">期間の開始日時</param>
            /// <param name="endOfPeriod">期間の終了日時</param>
            /// <returns></returns>
            public List<SelectListItem> GetTripNameFromWorkDays(DateTime workDay)
        {
            List<SelectListItem> tripRecordList = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectTripNameFromWorkDays(workDay);
                // DB接続
                tripRecordList = LoadTransitionConnectController.ConnectTTripRecordsForTripName(sql);

                return tripRecordList;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripRecordList;
            }
        }
    }
}
