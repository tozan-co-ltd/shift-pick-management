using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using static ai_truck_load_measurement.Models.TopModel;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// トップ画面
    /// </summary>
    public class TopController : BaseController
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        public IActionResult Index()
        {
            TopModel topModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // ハンディエラーメッセージ取得
                var sql = D_HandyErrorMessageConnectController.CreateSQLToSelectDHandyErrorMessages();
                IEnumerable<D_HandyErrorMessageModel> handyErrorMessageList = D_HandyErrorMessageConnectController.ConnectDHandyErrorMessages(sql, user.DatabaseName);
                topModel.D_HandyErrorMessageList = handyErrorMessageList.ToPagedList();

                // 表示する出荷作業進捗は、会社コード=10001(豊田自動織機)固定
                var mCompany = M_CompanyConnectController.GetMCompanyByCompanyCode("10001", user.DatabaseName);

                // 納入指示日は翌日(土日を除く)
                var nextDay = Utils.GetNextday(DateTime.Now);
                topModel.NextDayGraph = GetGraphInfo(user.MainDepoID, mCompany.CompanyID, nextDay.ToString("yyyy/MM/dd"), user.DatabaseName);
                // 納入指示日は翌々日(土日を除く)
                var nextTwoDay = Utils.GetNextday(nextDay);
                topModel.NextTwoDayGraph = GetGraphInfo(user.MainDepoID, mCompany.CompanyID, nextTwoDay.ToString("yyyy/MM/dd"), user.DatabaseName);

                // 項目設定
                topModel.DepoName = string.Concat(user.MainDepoName, " / ", mCompany.CompanyName, "向け");

                return View(topModel);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(topModel);
            }
        }

        /// <summary>
        /// グラフデータを作成
        /// </summary>
        /// <param name="mainDepoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="searchDate">検索日</param>
        /// <param name="databaseName">データベース</param>
        public GraphInfo GetGraphInfo(int mainDepoId, int companyId, string searchDate, string databaseName)
        {
            var graphInfo = new GraphInfo();
            try 
            {
                // 出荷指示情報取得
                var shipmentSql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedulesForWorkProgressInformation(mainDepoId, companyId, searchDate);
                IEnumerable<D_ShipmentScheduleModel> searchList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(shipmentSql, databaseName);
                var ShipmentScheduleList = searchList.ToPagedList();

                graphInfo.GraphTitle = string.Concat(searchDate, "分　出荷検品数");
                if (ShipmentScheduleList.Count > 0)
                {
                    // 検品済数と未検品数を取得
                    int scheduleTotal = 0;
                    int storeOutTotal = 0;
                    // 計算
                    foreach (var item in ShipmentScheduleList)
                    {
                        scheduleTotal += item.ScheduleNumberOfBoxes;
                        storeOutTotal += item.StoreOutNumberOfBoxes;
                    }
                    graphInfo.ShipmentScheduleTotal = scheduleTotal;
                    graphInfo.StoreOutTotal = storeOutTotal;
                }
                return graphInfo;
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}
