using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadTransitionModel();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectTripNameFromPeriod();
                // DB接続
                List<SelectListItem> tripRecordList = LoadTransitionConnectController.ConnectTTripRecordsForTripName(sql, "AI-truck-load-measurement_test");

                model.TripNameList = tripRecordList;
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }

        public List<int> GetTripBranchSeqFromTripName(string tripName)
        {
            List<int> tripBranchSeqList = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectTripBranchSeqFromTripName(tripName);
                // DB接続
                tripBranchSeqList = LoadTransitionConnectController.ConnectTTripRecordsForTripBranchSeq(sql, "AI-truck-load-measurement_test");

                return tripBranchSeqList;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripBranchSeqList;
            }
        }

        public List<RequestLoadStatus> SearchTrips(string tripName, int tripBranchSeq, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            List<RequestLoadStatus> loadStatuses = new ();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectLoadClassFromSearchConditions(tripName, tripBranchSeq, startOfPeriod, endOfPeriod);
                // DB接続
                var loadClasses = LoadTransitionConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

                foreach ( var loadClass in loadClasses)
                {
                    var loadStatus = new RequestLoadStatus
                    {
                        WorkDay = loadClass.WorkDay,
                        ArrivalLoadStatus = ConversionLoadClassToLoadStatus(loadClass.ArrivalLoadClass),
                        DepartureLoadStatus = ConversionLoadClassToLoadStatus(loadClass.DepartureLoadClass)
                    };
                    loadStatuses.Add(loadStatus);
                }
                return loadStatuses;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return loadStatuses;
            }
        }


        /// <summary>
        /// 荷量クラスからパーセント表示に変換
        /// </summary>
        /// <param name="loadClass">荷量クラス</param>
        /// <returns></returns>
        private string ConversionLoadClassToLoadStatus(int loadClass)
        {
            var loadStatus = "";
            if (loadClass >= 3)
            { 
                loadStatus = ((loadClass - 3) * 10 + 5).ToString() ;
            }
            return loadStatus;
        }
    }
}
