using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    public class D_ShipmentScheduleController : BaseController
    {
        /// <summary>
        /// 出荷指示照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(D_ShipmentScheduleModel model)
        {
            if (model == null)
                model = new D_ShipmentScheduleModel();

            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();

            // 管理権限区分が1(管理者)でない場合はエラーとする
            if (user == null || user.AuthorizedKubun != 1)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                return View(model);
            }

            // 出荷指示テーブル情報取得SQL作成
            var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();

            // DB接続
            IEnumerable<D_ShipmentScheduleModel> shipmentScheduleList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(sql, user.DatabaseName);

            model.D_ShipmentScheduleList = shipmentScheduleList.ToPagedList();

            return View(model);
        }
    }
}
