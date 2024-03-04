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

                // 倉庫IDで倉庫情報を取得
                var searchDepoSql = M_DepoConnectController.CreateSQLToSelectByDepoId(depoId);
                List <M_DepoModel> depoSearchList = M_DepoConnectController.ConnectMDepos(searchDepoSql, user.DatabaseName);
                if (depoSearchList.Count != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }
                // 会社IDで会社情報を取得
                var searchCompanySql = M_CompanyConnectController.CreateSQLToSelectByCompanyId(companyId);
                List<M_CompanyModel> companySearchList = M_CompanyConnectController.ConnectMCompanys(searchCompanySql, user.DatabaseName);
                if (companySearchList.Count != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }

                // 出荷実績情報取得SQL作成
                string sql = D_ShipmentConnectController.CreateSQLToSelectDShipments(
                    shipmentScheduleId, depoId, companyId);

                // 出荷実績情報取得
                List<D_ShipmentModel> searchList = D_ShipmentConnectController.ConnectDShipments(sql, user.DatabaseName);
                if (searchList.Count <= 0)
                {
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                    return View(model);
                }
                
                model.DShipmentList = searchList;
                model.DepoName = depoSearchList[0].DepoName;
                model.DepoID = depoId;
                model.DeliveryName = string.Concat(companySearchList[0].CompanyName, " - ", companySearchList[0].ClientName);
                model.DeliveryID = companyId;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }
    }
}
