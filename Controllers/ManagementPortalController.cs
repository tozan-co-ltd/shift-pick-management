using shift_pick_management.Commons;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc;

namespace shift_pick_management.Controllers
{
    public class ManagementPortalController : BaseController
    {
        public IActionResult Index()
        {
            var model = new ManagementPortalModel();
            return View(model);
        }

        /// <summary>
        /// 出発実績無し件数取得
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
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

        /// <summary>
        /// 紐づけ切れ-ID有 TOP10 取得
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
        public List<CountNonTripNameRecordModel> GetCountNonTripNameRecords(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var countNonTripNameRecords = new List<CountNonTripNameRecordModel>();
            try
            {
                var sql = ManagementPortalConnectController.CreateSQLToSelectCountNonTripNameRecordGroupByIdentifyNumber(startOfPeriod, endOfPeriod, checkedDepos);
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

        /// <summary>
        /// 便別紐づけ切れ回数 TO10 取得
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public List<CountNonTripNameRecordModel> GetCountNonTripNameAndInTripMasterRecords(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var countNonTripNameRecords = new List<CountNonTripNameRecordModel>();
            try
            {
                var sql = ManagementPortalConnectController.CreateSQLToSelectCountNonTripNameRecordAndInTripMaster(startOfPeriod, endOfPeriod, checkedDepos);
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

        /// <summary>
        /// 便取得
        /// </summary>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
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

        /// <summary>
        /// 紐づけ切れ原因割合比取得
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
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
                allRemarksCount += item.UnlinkedReasonCount;

            foreach(var item in modelList)
            {
                var remarkCount = item.UnlinkedReasonCount;
                item.RemarkPercentage = Math.Ceiling((double)remarkCount  / (double)allRemarksCount * 1000) / 10;
            }


            return modelList;
        }
    }
}
