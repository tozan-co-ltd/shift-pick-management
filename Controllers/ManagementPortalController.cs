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

        public RemarksAndCountNonTripNameRemarkModel GetCountNonTripNameRemarks(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var model = new RemarksAndCountNonTripNameRemarkModel();
            var countNonTripNameRemarks = new List<CountNonTripNameRemarkModel>();
            try
            {
                // データ取得
                var countSql = ManagementPortalConnectController.CreateSQLToSelectNonTripNameRemarkCount(startOfPeriod, endOfPeriod, checkedDepos);
                countNonTripNameRemarks = ConnectToSQLServer.ExecuteQueryToList<CountNonTripNameRemarkModel>(countSql);
                var remarksSql = ManagementPortalConnectController.CreateSQLToSelectRemarks();
                var remarks = ConnectToSQLServer.ExecuteQueryToList<string>(remarksSql);

                // 紐づけ切れ理由の割合取得
                var nonTripNameRemarks = GetRemarkPercentageFromDepo(countNonTripNameRemarks, checkedDepos);

                model.AllRemarks = remarks;
                model.NonTripNameRemarks = nonTripNameRemarks;
                return model;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return model;
            }
        }

        private List<CountNonTripNameRemarkModel> GetRemarkPercentageFromDepo(List<CountNonTripNameRemarkModel> modelList, List<string> checkedDepos)
        {
            var returnList = new List<CountNonTripNameRemarkModel>();
            foreach(var depoID in checkedDepos)
            {
                var modelListInDepo = modelList.FindAll(x => x.DepoID.ToString() == depoID);
                var getRemarkpercentageList = GetRemarkPercentage(modelListInDepo);
                returnList.AddRange(getRemarkpercentageList);
            }
            return returnList;
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
                item.RemarkPercentage = (double)remarkCount  / (double)allRemarksCount * 100;
            }


            return modelList;
        }
    }
}
