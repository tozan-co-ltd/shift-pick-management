using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class ManagementPortalController : BaseController
    {
        public IActionResult Index()
        {
            var model = new ManagementPortalModel();
            return View(model);
        }

        public List<NonDepartedAtRecordModel> GetNonDepartedAtRecords (DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var nonDepartedAtRecords = new List<NonDepartedAtRecordModel>();
            try
            {
                nonDepartedAtRecords = NonDepartedAtRecordConnectController.GetNonDepartedAtRecords(startOfPeriod, endOfPeriod, checkedDepos);
                return nonDepartedAtRecords;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return nonDepartedAtRecords;
            }
        }

        public List<CountNonTripNameRecordModel> GetCountNonTripNameRecords(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var countNonTripNameRecords = new List<CountNonTripNameRecordModel>();
            try
            {
                var sql = ManagementPortalConnectController.CreateSQLToSelectNonTripNameRecordCount(startOfPeriod, endOfPeriod, checkedDepos);
                countNonTripNameRecords = ConnectToSQLServer.ExecuteQueryToList<CountNonTripNameRecordModel>(sql);
                return countNonTripNameRecords;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return countNonTripNameRecords;
            }
        }

        public List<M_TripModel> GetTrips(List<string> checkedDepos)
        {
            var trips = new List<M_TripModel>();
            try
            {
                var sql = M_TripConnectController.CreateSQLToSelectMTrips(false, checkedDepos);
                trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
                return trips;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return trips;
            }
        }

        public List<CountNonTripNameRemarkModel> GetCountNonTripNameRemarks(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var countNonTripNameRemarks = new List<CountNonTripNameRemarkModel>();
            try
            {
                // データ取得
                var countSql = ManagementPortalConnectController.CreateSQLToSelectNonTripNameRemarkCount(startOfPeriod, endOfPeriod, checkedDepos);
                countNonTripNameRemarks = ConnectToSQLServer.ExecuteQueryToList<CountNonTripNameRemarkModel>(countSql);

                // 紐づけ切れ理由の割合取得
                var nonTripNameRemarks = GetRemarkPercentage(countNonTripNameRemarks);
                return nonTripNameRemarks;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return countNonTripNameRemarks;
            }
        }

        /// <summary>
        /// 紐づけ切れ理由の割合取得
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        private List<CountNonTripNameRemarkModel> GetRemarkPercentage(List<CountNonTripNameRemarkModel> modelList)
        {
            var allRemarksCount = 0;
            foreach(var item in modelList)
                allRemarksCount += item.RemarkCount;

            foreach(var item in modelList)
            {
                var remarkCount = item.RemarkCount;
                item.RemarkPercentage = Math.Ceiling((double)remarkCount  / (double)allRemarksCount * 1000) / 10;
            }


            return modelList;
        }
    }
}
