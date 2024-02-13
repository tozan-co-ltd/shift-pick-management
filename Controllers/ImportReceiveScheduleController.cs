using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class ImportReceiveScheduleController : BaseController
    {
        public IActionResult Index()
        {
            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();

            // 管理権限区分が1(管理者)でない場合はエラーとする
            if (user == null || user.AuthorizedKubun != 1)
            {
                // エラーメッセージ取得
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }

            var model = new D_FileImportModel();

            // 出荷指示取込一覧取得
            var listD_FileImport = GetListD_FileImport(ClaimsLoginUserData().DatabaseName);
            model.D_FileImportList = listD_FileImport;

            return View(model);
        }

        /// <summary>
        /// 出荷指示取込一覧取得
        /// </summary>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<D_FileImportModel> GetListD_FileImport(string databaseName)
        {
            // SQL作成
            var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport();

            // DB接続
            List<D_FileImportModel> strList = D_FileImportConnectController.ConnectD_FileImport(sql, databaseName);

            return strList;
        }
    }
}
