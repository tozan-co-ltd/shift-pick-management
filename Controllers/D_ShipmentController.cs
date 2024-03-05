using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出荷実績照会画面
    /// </summary>
    public class D_ShipmentController : BaseController
    {
        /// <summary>
        /// 出荷実績照会画面表示
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <returns></returns>
        public IActionResult Index(int shipmentScheduleId, int depoId, int companyId)
        {
            D_ShipmentModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫IDから倉庫情報を取得
                var searchDepoSql = M_DepoConnectController.CreateSQLToSelectByDepoId(depoId);
                List <M_DepoModel> depoSearchList = M_DepoConnectController.ConnectMDepos(searchDepoSql, user.DatabaseName);

                // 会社IDから会社情報を取得
                var searchCompanySql = M_CompanyConnectController.CreateSQLToSelectByCompanyId(companyId);
                List<M_CompanyModel> companySearchList = M_CompanyConnectController.ConnectMCompanys(searchCompanySql, user.DatabaseName);

                // 出荷実績情報取得
                string sql = D_ShipmentConnectController.CreateSQLToSelectDShipments(
                    shipmentScheduleId, depoId, companyId);
                List<D_ShipmentModel> dShipmentList = D_ShipmentConnectController.ConnectDShipments(sql, user.DatabaseName);
                
                model.DShipmentList = dShipmentList;
                model.DepoName = depoSearchList[0].DepoName;
                model.DeliveryName = string.Concat(companySearchList[0].CompanyName, " - ", companySearchList[0].ClientName);

                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }
    }
}
