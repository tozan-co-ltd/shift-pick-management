using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class D_ShipmentScheduleController : BaseController
    {
        /// <summary>
        /// 出荷指示照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();

            //// 出荷指示テーブル情報取得SQL作成
            //var sql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedules();
            //// DB接続
            //List<D_ShipmentScheduleModel> model = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(sql, user.DatabaseName);

            var model = new D_ShipmentScheduleModel();
            return View(model);
        }
    }
}
