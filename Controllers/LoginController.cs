using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;
using System.DirectoryServices;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

namespace ai_truck_load_measurement.Controllers
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
        public IActionResult Index(string param)
        {
            try
            {
                // 開発環境の場合はViewDataに"true"を代入し、
                // _LayoutLogin.cshtmlで背景の色を変更(薄紫#EFEDFF)
                ViewData["IsDevelopment"] = null;
#if DEBUG
                ViewData["IsDevelopment"] = "true";
#endif

                return View();
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;
                return View();
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

                    return View();
                }

                // 現在時刻取得
                var dateTime = DateTime.Now;

                // クレーム作成
                // ユーザー情報をクレームに追加
                var claims = new[] {
                    new Claim("UserName", loginUserModel.UserName),
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

                return RedirectToAction("Index", "Top");
            }
            catch (Exception ex)
            {
                errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;

                // log取得
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return View();
            }
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

                // ActiveDirectory認証処理
                var authenticateUserName = GetAuthenticateUserName(loginId, password);
                if(authenticateUserName == null)
                {
                    return null;
                }

                LoginUserModel loginUserModel = new()
                {
                    UserName = authenticateUserName,
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
    }
}
