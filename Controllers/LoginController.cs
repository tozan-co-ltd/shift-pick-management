using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
//using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.Extensions.Logging;

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
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}
