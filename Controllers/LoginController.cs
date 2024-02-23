using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// ログイン関係
    /// </summary>
    public class LoginController : Controller
    {
        //private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ログイン画面表示
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index(string param)
        {
            try
            {
                // 強制ログアウトの場合はエラーメッセージ表示
                if (param == "autologout")
                {
                    ViewData["ErrorMessage"] = "異なるログインを検出したため自動ログアウトされました。";
                }

                // 開発環境("_test"が含まれている)の場合はViewDataに"true"を代入し、
                // _LayoutLogin.cshtmlで背景の色を変更(薄紫#EFEDFF)
                ViewData["IsDevelopment"] = null;
                string companyWebPath = GetCompanyWebPathByURL();
                if (companyWebPath.Contains("_test"))
                {
                    ViewData["IsDevelopment"] = "true";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex;
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
            try
            {
                // ログインの入力値 チェック
                LoginUserModel loginUserModel = CheckInputValuesForLogin(model);

                // エラー入力の場合
                if (loginUserModel == null)
                {
                    ViewData["ErrorMessage"] = "ログインIDまたはパスワードが正しくありません。";
                    return View();
                }

                // 現在時刻取得
                var dateTime = DateTime.Now;
                string timeStamp = dateTime.ToString();

                // クレーム作成
                // ユーザー情報をクレームに追加する
                var claims = new[] {
                    new Claim("CompanyID", loginUserModel.CompanyID.ToString()),
                    new Claim("CompanyCode", loginUserModel.CompanyCode),
                    new Claim("CompanyName", loginUserModel.CompanyName),
                    new Claim("DatabaseName", loginUserModel.DatabaseName),
                    new Claim("UserID", loginUserModel.UserID.ToString()),
                    new Claim("UserName", loginUserModel.UserName),
                    new Claim("Role", loginUserModel.Role.ToString()),
                    new Claim("MainDepoID", loginUserModel.MainDepoID.ToString()),
                    new Claim("MainDepoName", loginUserModel.MainDepoName),
                    new Claim("AuthorizedKubun", loginUserModel.AuthorizedKubun.ToString()),
                    new Claim("TimeStamp", timeStamp),
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

                // ログインフラグ=1,最終ログイン日時更新
                // SQL作成
                var sql = LoginConnectController.CreateSQLToUpdateMUserByLogin(loginUserModel.UserID, dateTime);
                // DB接続
                M_UserConnectController.ConnectMUsers(sql, loginUserModel.DatabaseName);

                // log取得
                //Logger.Info($"ログイン成功 ログインユーザー名:{mUsersModel.UserName}");

                return RedirectToAction("Index", "Top");
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message;
                ViewData["ErrorMessage"] = errorMessage;

                // log取得
                //var exceptionMessage = ex.Message;
                //Logger.Error($"{exceptionMessage} {errorMessage}");

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

                // ログインフラグ=0に更新
                if (claimsList.Count > 0)
                {
                    int userID = Convert.ToInt32(User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value);
                    string databaseName = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;

                    // SQL作成
                    var sql = LoginConnectController.CreateSQLToUpdateMUserByLogout(userID);
                    // DB接続
                    M_UserConnectController.ConnectMUsers(sql, databaseName);
                }

                // サインアウト
                // レスポンスから認証クッキーを削除
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // 強制ログアウトの場合はエラーメッセージ表示
                if (param == "autologout")
                {
                    return RedirectToAction("Index", new { param = "autologout" });
                }

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
        /// URLから会社WEBアプリパス取得
        /// </summary>
        /// <returns>会社WEBアプリパス</returns>
        private string GetCompanyWebPathByURL()
        {
            string companyWebPath = "";
            try
            {
                // URLからパスを取得(https://www.tozan.co.jp/の直後１つ目のパス)
                var urlWebPath = HttpContext.Request.PathBase.ToString().Substring(1);

                // 会社WEBアプリパスを取得("sumaken-web-***"の"***"のみ)
                string pattern = "sumaken-web-";

                int num = urlWebPath.IndexOf(pattern);
                if (num != -1)
                {
                    companyWebPath = urlWebPath.Substring(num + pattern.Length);
                }
                return companyWebPath;
            }
            catch (Exception)
            {
                throw;
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

                // 入力規則チェック
                // ログインIDまたはパスワードが空欄、パスワードが4桁未満または10桁を超える場合はエラー
                if (!ModelState.IsValid)
                {
                    return null;
                }

                // 会社マスターからデータベース名取得
                var mCompany = GetMCompany();
                if (mCompany == null ||  string.IsNullOrWhiteSpace(mCompany.DatabaseName))
                {
                    return null;
                }

                // ログインユーザー情報取得
                var sql = LoginConnectController.CreateSQLToSelectMUserByLoginUser(loginId);
                List<M_UserModel> mUsers = M_UserConnectController.ConnectMUsers(sql, mCompany.DatabaseName);
                M_UserModel? mUser = mUsers.FirstOrDefault();
                if (mUser == null)
                {
                    return null;
                }

                // 入力されたパスワードをハッシュ化
                byte[] salt = Hashing.ConvertStringToBytes(mUser.Salt);
                string hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(password, salt);

                // ハッシュ化されたパスワードと一致するかチェック
                if (!mUser.Password.Equals(hashedPassword))
                {
                    return null;
                }

                LoginUserModel loginUserModel = new()
                {
                    CompanyID = mCompany.CompanyID,
                    CompanyCode = mCompany.CompanyCode,
                    CompanyName = mCompany.CompanyName,
                    DatabaseName = mCompany.DatabaseName,
                    UserID = mUser.UserID,
                    UserName = mUser.UserName,
                    Role = mUser.Role,
                    MainDepoID = mUser.DepoID,
                    MainDepoName = mUser.DepoName,
                    AuthorizedKubun = mUser.AuthorizedKubun
                };

                return loginUserModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社マスター情報取得
        /// </summary>
        /// <returns></returns>
        private Warehouse_M_CompanyModel? GetMCompany()
        {
            try
            {
                string companyWebPath = GetCompanyWebPathByURL();

                if (companyWebPath != "")
                {
                    // SQL作成
                    var sql = Warehouse_M_CompanyConnectController.CreateSQLToSelectMCompanyByWebPath(companyWebPath);
                    // DB接続
                    Warehouse_M_CompanyModel? companyModel = Warehouse_M_CompanyConnectController.ConnectMCompanny(sql);

                    return companyModel;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
