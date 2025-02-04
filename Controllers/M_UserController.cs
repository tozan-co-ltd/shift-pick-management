using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using ai_truck_load_measurement.Commons;
using System.Data;

namespace ai_truck_load_measurement.Controllers
{
    public class M_UserController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ユーザーマスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_UserModel model = new();

            try
            {
                // 便マスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToSelectMUsers();
                // DB接続
                IEnumerable<M_UserModel> userList = M_UserConnectController.ConnectMUsers(sql);

                model.M_UserList = userList.ToPagedList();
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
        /// ユーザーマスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_UserModel model = new();
            try
            {
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// ユーザーマスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_UserModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"車両マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // AD名重複チェック
                var duplicateCheck = ADNameDuplicateCheck(model);
                if (duplicateCheck != null)
                {
                    errorMessage = duplicateCheck;

                    return NotFound(new { errorMessage });
                }

                // 車両マスター登録
                M_UserConnectController.InsertMUser(model, user);

                // log取得
                _logger.Info($"車両マスター登録成功 ユーザー名:{model.ADName}");

                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
        }

        /// <summary>
        /// AD名重複チェック
        /// </summary>
        /// <param name="model">チェック対象</param>
        /// <returns></returns>
        private string? ADNameDuplicateCheck(M_UserModel model)
        {
            var sql = M_UserConnectController.CreateSQLToSelectDuplicateADName(model);
            bool isExistedADName = ConnectToSQLServer.IsExistedSameRecord(sql);
            if (isExistedADName)
            {
                string displayName = Utils.GetDisplayName<M_UserModel>("ADName");

                // log取得
                var errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, displayName);
                _logger.Error($"車両マスター登録失敗 {errorMessage}");

                return errorMessage;
            }

            return null;
        }
    }
}