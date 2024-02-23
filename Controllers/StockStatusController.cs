using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 在庫照会画面
    /// </summary>
    public class StockStatusController : BaseController
    {
        /// <summary>
        /// 在庫照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            StockStatusModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社リスト取得
                CommonModel commonModel = new();
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_SupplierID);

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
