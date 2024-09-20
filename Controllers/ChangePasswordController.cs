using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// パスワード変更画面
    /// </summary>
    public class ChangePasswordController : BaseController
    {
        private readonly ILogger<ChangePasswordController> _logger;

        public ChangePasswordController(ILogger<ChangePasswordController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// パスワード変更画面表示
        /// </summary>
        public IActionResult Index()
        {
            ChangePasswordModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                model.UserID = user.UserID;
                model.UserName = user.UserName;

                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// パスワード更新
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(ChangePasswordModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // saltの作成とパスワードのハッシュ化
                var salt = Hashing.GetRandomSalt();
                var hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(model.Password, salt);
                var stringSalt = Hashing.ConvertByteToString(salt);
                model.Password = hashedPassword;
                model.Salt = stringSalt;
                
                // ユーザーマスター更新
                M_UserConnectController.UpdateMUserPassword(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
            }
        }
    }
}
