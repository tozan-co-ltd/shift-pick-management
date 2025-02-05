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
                // ユーザーマスター情報取得SQL作成
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
                    _logger.Error($"ユーザーマスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // AD名重複チェック
                var duplicateCheck = IsADNameDuplicate(model);
                if (duplicateCheck)
                {
                    string displayName = Utils.GetDisplayName<M_UserModel>("ADName");

                    // log取得
                    errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, displayName);
                    _logger.Error($"ユーザーマスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // ユーザーマスター登録
                M_UserConnectController.InsertMUser(model, user);

                // log取得
                _logger.Info($"ユーザーマスター登録成功 ユーザー名:{model.ADName}");

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
        /// ユーザーマスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_UserModel model)
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
                    _logger.Error($"ユーザーマスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // ユーザーコード重複チェック
                var duplicateCheck = IsADNameDuplicate(model);
                if (duplicateCheck)
                {
                    string displayName = Utils.GetDisplayName<M_UserModel>("ADName");

                    // log取得
                    errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, displayName);
                    _logger.Error($"ユーザーマスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // ユーザーマスター更新
                M_UserConnectController.UpdateMUser(model, user);

                // log取得
                _logger.Info($"ユーザーマスター更新成功 ユーザーID:{model.UserID}");

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
        /// ユーザーマスター削除
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        public IActionResult Delete(int userId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // ユーザーマスター削除
                int deleteAffectedRows = M_UserConnectController.DeleteMUser(userId, user);

                // log取得
                _logger.Info($"ユーザーマスター削除成功 ユーザーID:{userId}");

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
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName)
        {
            string? errorMessage;
            try
            {
                // ユーザーマスター情報取得
                var sql = M_UserConnectController.CreateSQLToSelectMUsersForDataTable();
                DataTable dt = M_UserConnectController.ConnectMUsersToDataTable(sql);

                // 管理権限列を数字から文字に変換
                var conversionedDt = ConvertAuthorizedKubunFromNumberToString(dt);

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(gamenName);
                // 2シートあり
                bool sheetTwo = false;


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(conversionedDt, null, tmpFilename, sheetTwo, null, null, gamenName);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);


                        return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
                    }
                    else
                    {
                        // エラーメッセージ取得
                        // 「ファイルが存在しません。」
                        errorMessage = ErrorMessagesResources.E9999;

                        return Json(new { res = "NG", error = errorMessage });
                    }
                }
                catch (Exception ex)
                {
                    // エラーメッセージ取得
                    // 「NASに接続できませんでした。」
                    errorMessage = ErrorMessagesResources.E9999;

                    // log取得
                    var exceptionMessage = ex.Message;
                    return Json(new { res = "NG", error = errorMessage + exceptionMessage });
                }
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;

                // log取得
                var exceptionMessage = ex.Message;
                return Json(new { res = "NG", error = errorMessage + exceptionMessage });
            }

        }

        /// <summary>
        /// AD名重複チェック
        /// </summary>
        /// <param name="model">チェック対象</param>
        /// <returns></returns>
        private bool IsADNameDuplicate(M_UserModel model)
        {
            var sql = M_UserConnectController.CreateSQLToSelectDuplicateADName(model);
            bool isExistedADName = ConnectToSQLServer.IsExistedSameRecord(sql);
            if (isExistedADName)
                return true;

            return false;
        }

        /// <summary>
        /// データテーブルの管理権限列を数字から文字に変換する
        /// </summary>
        /// <param name="dt">変換元データテーブル</param>
        /// <returns></returns>
        private DataTable ConvertAuthorizedKubunFromNumberToString(DataTable dt)
        {
            var index = dt.Columns.IndexOf("authorized_kubun");
            dt.Columns.Add("authorized_kubun_name").SetOrdinal(index);
            foreach (DataRow row in dt.Rows)
            {
                var authorizedKubun = (int)row["authorized_kubun"];
                if (authorizedKubun == 0)
                {
                    row["authorized_kubun_name"] = "管理者";
                }
                else if (authorizedKubun == 1)
                {
                    row["authorized_kubun_name"] = "なし";
                }
            }
            dt.Columns.Remove("authorized_kubun");

            return dt;
        }
    }
}