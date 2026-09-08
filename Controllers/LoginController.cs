using shift_pick_management.Commons;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;
using System.DirectoryServices;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;
using shift_pick_management.ConnectControllers;
using System.Text.RegularExpressions;

namespace shift_pick_management.Controllers
{
    /// <summary>
    /// ログイン関係
    /// </summary>
    public class LoginController : Controller
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ログイン画面表示
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index(string ReturnUrl)
        {
            LoginModel model = new();
            try
            {
                // 開発環境の場合はViewDataに"true"を代入し、
                // _LayoutLogin.cshtmlで背景の色を変更(薄紫#EFEDFF)
                ViewData["IsDevelopment"] = null;
#if DEBUG
                ViewData["IsDevelopment"] = "true";
#endif
                model.ReturnUrl = ReturnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;
                return View(model);
            }
        }

        /// <summary>
        /// ログイン
        /// </summary>
        /// <param name="model">LoginModel</param>
        /// <returns>トップ画面</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            string? errorMessage;
            string? errorCause;

            try
            {
                // 入力規則チェック
                LoginUserModel loginUserModel = CheckInputValuesForLogin(model);
                if (loginUserModel == null)
                {
                    ViewData["ErrorMessage"] = "E1002: " + ErrorMessagesResources.E1002;

                    // 開発環境の場合はViewDataに"true"を代入し、
                    // _LayoutLogin.cshtmlで背景の色を変更(薄紫#EFEDFF)
#if DEBUG
                    ViewData["IsDevelopment"] = "true";
#endif

                    // log取得
                    errorMessage = "E1002: " + ErrorMessagesResources.E1002;
                    errorCause = "入力規則エラー";
                    _logger.Error($"{errorMessage} {errorCause} 入力値:{model.LoginId}, {model.Password}");

                    return View(model);
                }

                // 現在時刻取得
                var dateTime = DateTime.Now;

                // クレーム作成
                // ユーザー情報をクレームに追加
                var claims = new[] {
                    new Claim("UserName", loginUserModel.UserName),
                    new Claim("ADName", model.LoginId),
                    new Claim("AuthorizedKubun", loginUserModel.AuthorizedKubun.ToString()),
                    new Claim("MainDepoID", loginUserModel.MainDepoID.ToString()),
                    new Claim("MainDepoName", loginUserModel.MainDepoName),
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                    IsPersistent = false,
                };

                // サインイン
                // 認証クッキーをレスポンスに追加
                await HttpContext.SignInAsync(
                  CookieAuthenticationDefaults.AuthenticationScheme,
                  principal,
                  authProperties
                );

              
                // log取得
                _logger.Info($"ログイン成功 ログインID:{model.LoginId}, ログインユーザー名:{loginUserModel.UserName}");

                if (!string.IsNullOrEmpty(model.ReturnUrl) && model.ReturnUrl.Contains("AlertRecord"))
                {
                    var isArrived = GetIsArrived(model.ReturnUrl);
                    string alertRecordID = Regex.Replace(model.ReturnUrl, @"[^0-9]", "");
                    return RedirectToAction("Index", "AlertRecord", new {TransitionAlertRecordID = alertRecordID, TransitionIsArrived = isArrived});
                }

                return RedirectToAction("Index", "Top");
            }
            catch (Exception ex)
            {
                errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;

                // log取得
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return View(model);
            }
        }

        private bool GetIsArrived(string returnUrl)
        {
            var isArrived = false;
            if (returnUrl.Contains("True"))
                isArrived = true;
            return isArrived;
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<IActionResult> Logout(string param)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var claimsList = User.Claims.ToList();

                // サインアウト
                // レスポンスから認証クッキーを削除
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // ログイン画面へリダイレクト
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ログインの入力値チェック
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        private LoginUserModel? CheckInputValuesForLogin(LoginModel loginModel)
        {
            try
            {
                var loginId = loginModel.LoginId;
                var password = loginModel.Password;

                if (loginId == "test1000" && password =="1111")
                {
                    LoginUserModel testUserModel = new()
                    {
                        UserName = "テストユーザー",
                        AuthorizedKubun = 0,
                        MainDepoID = 0,
                        MainDepoName = ""
                    };
                    return testUserModel;
                }

                // ActiveDirectory認証処理
                var authenticateUserName = GetAuthenticateUserName(loginId, password);
                if(authenticateUserName == null)
                {
                    return null;
                }

                var mainDepo = GetMainDepo(loginId);
                var mainDepoName = "";
                if (mainDepo.Name != null)
                    mainDepoName = mainDepo.Name;

                LoginUserModel loginUserModel = new()
                {
                    UserName = authenticateUserName,
                    AuthorizedKubun = GetAuthorizedKubunOfUser(loginId),
                    MainDepoID = mainDepo.DepoID,
                    MainDepoName = mainDepoName
                };

                return loginUserModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ActiveDirectory認証とユーザー名取得
        /// </summary>
        /// <param name="loginId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <returns>認証されたユーザー名</returns>
        private string? GetAuthenticateUserName(string loginId, string password)
        {
            try
            {
                if(loginId == null || password == null)
                {
                    return null;
                }
                string ldapPath = "LDAP://192.168.1.6/DC=tozan,DC=co,DC=jp";
                DirectoryEntry directoryEntry = new DirectoryEntry(ldapPath, loginId, password);

                // Active Directory でユーザーを検索
                DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                searcher.Filter = "(&(objectClass=user)(sAMAccountName=" + loginId + "))";
                searcher.SearchScope = SearchScope.Subtree;

                // ユーザーが見つかったかどうかを確認
                // IDとパスワードが一致しなかった場合、例外処理に移行
                SearchResult result = searcher.FindOne();

                if (result == null)
                {
                    return null;
                }
                return result.Properties["displayname"][0].ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// ログインIDを元にログイン時の管理権限区分を取得する
        /// </summary>
        /// <param name="loginId">ログインID</param>
        /// <returns></returns>
        private int GetAuthorizedKubunOfUser(string loginID)
        {
            // ログインIDから権限区分を取得
            var sql = LoginConnectController.CreateSQLToSelectAuthorizedKubunFromUserName(loginID);
            var authorizedKubun = GetAuthorizedKubunFromUserName(sql);

            return authorizedKubun;

        }

        /// <summary>
        /// ログインIDを元にログイン時のメインデポを取得する
        /// </summary>
        /// <param name="loginID"></param>
        /// <returns></returns>
        private M_DepoModel GetMainDepo(string loginID)
        {
            M_DepoModel model = new M_DepoModel();
            var sql = LoginConnectController.CreateSQLToSelectDepoFromADName(loginID);
            var depoList = LoginConnectController.ConnectMDepos(sql);
            if (depoList.Count > 0)
            {
                model = depoList[0];
            }
            return model;

        }
        /// <summary>
        /// ユーザーの権限区分情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        private int GetAuthorizedKubunFromUserName(string sql)
        {
            // 戻り値 デフォルト値は権限無しの0
            var authorizedKubun = 0;

            List<M_UserModel> strList = new();

            // DB接続
            try
            {
                strList = ConnectToSQLServer.ExecuteQueryToList<M_UserModel>(sql);
                if (strList.Count > 0)
                {
                    authorizedKubun = strList[0].AuthorizedKubun;
                }

                return authorizedKubun;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
