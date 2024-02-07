using mar_sumaken_web.Commons;
using Microsoft.AspNetCore.Mvc;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;

namespace mar_sumaken_web.Controllers
{
    public class M_CompanyController : BaseController
    {
        private readonly ILogger<M_CompanyController> _logger;

        public M_CompanyController(ILogger<M_CompanyController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 会社マスターリスト取得
        /// </summary>
        public IActionResult Index()
        {
            List<M_CompanyModel> listCompany = new List<M_CompanyModel>();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View(listCompany);
                }

                // ユーザーマスター情報取得SQL作成
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();
                // DB接続
                List<M_CompanyModel> companyList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);
                if (companyList.Count > 0)
                {
                    listCompany = companyList;
                }

                return View(listCompany);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(listCompany);
            }
        }

        /// <summary>
        /// 会社マスターを削除
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <returns></returns>
        public IActionResult Delete(int companyId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || companyId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // ユーザーマスター削除
                int deleteAffectedRows = M_CompanyConnectController.DeleteMCompany(companyId, user.DatabaseName);

                // 更新件数が0の場合はエラーとする
                if (deleteAffectedRows == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
                return NotFound(new { errorMessage = "予期せぬエラーが発⽣しました。" });
            }
        }
    }
}
