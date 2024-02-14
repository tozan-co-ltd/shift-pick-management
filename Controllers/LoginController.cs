using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// ログイン関係
    /// </summary>
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// ログイン画面表示
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
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
                LoginUserModel loginUserModel = this.CheckInputValuesForLogin(model);

                // エラー入力の場合
                if (loginUserModel == null)
                {
                    ViewData["ErrorMessage"] = "ログインIDまたはパスワードが正しくありません。";
                    return View();
                }

                // 現在時刻取得
                string now = DateTime.Now.ToString();

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
                    new Claim("TimeStamp", now),
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
                _logger.LogInformation($"ログイン成功 ログインユーザー名:{loginUserModel.UserName}");

                return RedirectToAction("Index", "Top");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex;
                return View();
            }
        }

        /// <summary>
        /// ログインの入力値 チェック
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        private LoginUserModel CheckInputValuesForLogin(LoginModel loginModel)
        {
            try
            {
                var loginId = loginModel.LoginId;
                var password = loginModel.Password;

                // (1) 入力規則チェック
                // ログインIDまたはパスワードが空欄
                // パスワードが4桁未満、または10桁を超える場合
                if (!ModelState.IsValid)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return null;
                }

                // (2) (3) 会社マスター情報の取得
                var mCompany = GetMCompany();
                if (mCompany == null ||  string.IsNullOrWhiteSpace(mCompany.DatabaseName))
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return null;
                }

                // ログインユーザー情報取得
                List<M_UserModel> mUsers = LoginController.GetMUserToLogin(mCompany.DatabaseName, loginId);
                M_UserModel? mUser = mUsers.FirstOrDefault();
                
                // 一致するデータが無い場合
                if (mUser == null)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return null;
                }

                // (4) 16進数文字列をbyte列に変換
                // (5) 平文パスワードをハッシュ化されたパスワードに変換
                // 入力されたパスワードをハッシュ化
                byte[] salt = Hashing.ConvertStringToBytes(mUser.Salt);
                string hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(password, salt);

                // パスワードチェック
                // ハッシュ化されたパスワードと一致するデータが無い場合
                if (!mUser.Password.Equals(hashedPassword))
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return null;
                }
                LoginUserModel loginUserModel = new LoginUserModel()
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
        /// ログインユーザー情報取得
        /// </summary>
        /// <param name="loginId">ログインID</param>
        /// <param name="authorizedKubunList">管理権限区分リスト</param>
        /// <returns>MUsersViewModel</returns>
        public static List<M_UserModel> GetMUserToLogin(string databaseName, string loginId)
        {
            try
            {
                // SQL作成
                var sql = LoginConnectController.CreateSQLToSelectMUerByLoginUser(loginId);
                // DB接続
                List<M_UserModel> userList = M_UserConnectController.ConnectMUsers(sql, databaseName);
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社マスター情報の取得
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private Warehouse_M_CompanyModel? GetMCompany()
        {
            string companyWebPath = "";
            try
            {
                // URLからパスを取得(https://www.tozan.co.jp/の直後１つ目のパス)
                var urlWebPath = HttpContext.Request.PathBase.ToString().Substring(1);

                // 会社Webアプリパスを取得("sumaken-web-***"の"***"のみ)
                string pattern = "sumaken-web-";
                int index = urlWebPath.IndexOf(pattern);
                if (index != -1)
                {
                    companyWebPath = urlWebPath.Substring(index + pattern.Length);
                }

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
