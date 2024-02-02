using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
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

        /// <summary>
        /// ユーザーマスター画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(M_UserModel model)
        {
            if (model == null)
                model = new M_UserModel();

            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    throw new Exception();
                }

                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToGetMUsers();
                // DB接続
                List<M_User> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                
                if (userList.Count > 0)
                {
                    // ユーザーマスターの詳細を取得する
                    userList = M_UserConnectController.GetMUserDetailList(userList, user.DatabaseName);
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

        /// <summary>
        /// ユーザーマスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// ユーザーマスター登録
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(M_User model)
        {
            return View();
        }

        /// <summary>
        /// ユーザーマスター削除
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IActionResult Delete(int userId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || userId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // ユーザーマスター削除
                int deleteAffectedRows = M_UserConnectController.DeleteMUser(userId, user.DatabaseName);

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
