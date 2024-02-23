using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
            D_HandyErrorMessageModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // SQL作成
                var sql = D_HandyErrorMessageConnectController.CreateSQLToGetDFileImport();
                // DB接続
                List<D_HandyErrorMessageModel> DFileImportList = D_HandyErrorMessageConnectController.ConnectDHandyErrorMessage(sql, user.DatabaseName);

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
