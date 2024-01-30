//using mar_sumaken_web.Commons;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Controllers
{
    public class M_UserController : BaseController
    {
        private readonly ILogger<M_UserController> _logger;

        public M_UserController(ILogger<M_UserController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(M_UserModel model)
        {
            string? errorMessage;

            if (model == null)
                model = new M_UserModel();

            try
            {
                // クレームからユーザー情報の管理権限区分を取得する
                var user = UserDataList();

                // 管理権限区分チェック
                // 1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1 || string.IsNullOrWhiteSpace(user.DatabaseName))
                {
                    // エラーを作成
                    // エラーコード：E2011
                    throw new Exception();
                }

                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToGetMUsers();
                // DB接続
                List<M_User> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                
                if (userList.Count > 0)
                {
                    // ユーザーマスターリストの詳細を取得する
                    userList = M_UserConnectController.GetUserListDetail(userList, user.DatabaseName);
                    model.M_UserList = userList;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
                return View();
            }
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
