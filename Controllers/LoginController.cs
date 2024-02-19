using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            try
            {
                ViewData["IsDevelopment"] = null;

                string companyWebPath = GetCompanyWebPathByURL();

                // 開発環境("_test"が含まれている)の場合はViewDataに"true"を代入し、
                // _LayoutLogin.cshtmlで背景の色を変更する(薄紫#EFEDFF)
                if (companyWebPath.Contains("_test"))
                {
                    ViewData["IsDevelopment"] = "true";
                }

                return View();
            }
            catch (Exception)
            {
                throw;
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

                int index = urlWebPath.IndexOf(pattern);
                if (index != -1)
                {
                    companyWebPath = urlWebPath.Substring(index + pattern.Length);
                }
                return companyWebPath;
            }
            catch (Exception)
            {
                throw;
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
        /// ログインの入力値チェック
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        private LoginUserModel CheckInputValuesForLogin(LoginModel loginModel)
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
                var sql = LoginConnectController.CreateSQLToSelectMUerByLoginUser(loginId);
                List<M_UserModel> mUsers = M_UserConnectController.ConnectMUsers(sql, mCompany.DatabaseName);
                M_UserModel? mUser = mUsers.FirstOrDefault();
                
                // 一致するデータが無い場合
                if (mUser == null)
                {
                    return null;
                }

                // 入力されたパスワードをハッシュ化
                byte[] salt = Hashing.ConvertStringToBytes(mUser.Salt);
                string hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(password, salt);

                // パスワードチェック
                // ハッシュ化されたパスワードと一致するデータが無い場合はエラー
                if (!mUser.Password.Equals(hashedPassword))
                {
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
