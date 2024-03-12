using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using X.PagedList;

namespace mar_sumaken_web.Controllers
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
                var nextDay = Utils.GetNextday(DateTime.Now).ToString("yyyy/MM/dd");

                // 出荷指示情報取得
                var shipmentSql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedulesForWorkProgressInformation(user.MainDepoID, mCompany.CompanyID, nextDay);
                IEnumerable<D_ShipmentScheduleModel> searchList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(shipmentSql, user.DatabaseName);
                topModel.ShipmentScheduleList = searchList.ToPagedList();

                if (topModel.ShipmentScheduleList.Count > 0)
                {
                    // 検品済数と未検品数を取得
                    int scheduleTotal = 0;
                    int storeOutTotal = 0;
                    // 計算
                    foreach (var item in topModel.ShipmentScheduleList)
                    {
                        scheduleTotal += item.ScheduleNumberOfBoxes;
                        storeOutTotal += item.StoreOutNumberOfBoxes;
                    }
                    topModel.ShipmentScheduleTotal = scheduleTotal;
                    topModel.StoreOutTotal = storeOutTotal;
                }
                // 項目設定
                topModel.DepoName = user.MainDepoName;
                topModel.GraphTitle = string.Concat(nextDay, "分　", mCompany.CompanyName, "向け", "　出荷検品数");

                return View(topModel);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(topModel);
            }
        }
    }
}
