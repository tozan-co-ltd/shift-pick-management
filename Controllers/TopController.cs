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
            D_ShipmentScheduleModel shipmentScheduleModel = new();
            D_HandyErrorMessageModel handyErrorMessageModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                //// 出荷指示報取得SQL作成
                //var shipmentSql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedulesForWorkProgressInformation();
                //// DB接続
                //IEnumerable<D_ShipmentScheduleModel> searchList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(shipmentSql, user.DatabaseName);
                //shipmentScheduleModel.D_ShipmentScheduleList = searchList.ToPagedList();

                //// SQL作成
                //var sql = D_HandyErrorMessageConnectController.CreateSQLToSelectDHandyErrorMessages();
                //// DB接続
                //IEnumerable<D_HandyErrorMessageModel> handyErrorMessageList = D_HandyErrorMessageConnectController.ConnectDHandyErrorMessages(sql, user.DatabaseName);
                //handyErrorMessageModel.D_HandyErrorMessageList = handyErrorMessageList.ToPagedList();
                //topModel = new TopModel
                //{
                //    MyModel1 = (IPagedList<D_ShipmentScheduleModel>)shipmentScheduleModel,
                //    MyModel2 = (IPagedList<D_HandyErrorMessageModel>)handyErrorMessageModel
                //};

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
