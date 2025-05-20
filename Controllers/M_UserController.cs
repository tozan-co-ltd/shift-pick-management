using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using ai_truck_load_measurement.Commons;
using System.Data;
using System.DirectoryServices;
using NPOI.SS.Formula.Functions;

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
                IEnumerable<M_UserModel> userList = ConnectToSQLServer.ExecuteQuery<M_UserModel>(sql);

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

                // 入力チェック
                var validCheck = ValidCheck(model);
                if (!validCheck.IsValid)
                {
                    // log取得
                    errorMessage = validCheck.ErrorMessage;
                    _logger.Error($"ユーザーマスター登録失敗 {errorMessage}");

                    return BadRequest(new { errorMessage });
                }

                // ユーザー名がADに存在するか
                // デバッグ時は無効化
#if DEBUG
#else
                if (!HasNameInAD(model.ADName))
                {
                    string displayName = Utils.GetDisplayName<M_UserModel>("ADName");
                    // log取得
                    errorMessage = "E1013: " + string.Format(ErrorMessagesResources.E1013, displayName);
                    _logger.Error($"ユーザーマスター更新失敗 {errorMessage}");

                    return BadRequest(new { errorMessage });
                }
#endif

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

                return BadRequest(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return BadRequest(new { errorMessage });
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

                // 入力チェック
                var validCheck = ValidCheck(model);
                if (!validCheck.IsValid)
                {
                    // log取得
                    errorMessage = validCheck.ErrorMessage;
                    _logger.Error($"ユーザーマスター登録失敗 {errorMessage}");

                    return BadRequest(new { errorMessage });
                }

                // ユーザー名がADに存在するか
                // デバッグ時は無効化
#if DEBUG
#else
                if (!HasNameInAD(model.ADName))
                {
                    string displayName = Utils.GetDisplayName<M_UserModel>("ADName");
                    // log取得
                    errorMessage = "E1013: " + string.Format(ErrorMessagesResources.E1013, displayName);
                    _logger.Error($"ユーザーマスター更新失敗 {errorMessage}");

                    return BadRequest(new { errorMessage });
                }
#endif

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

                return BadRequest(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return BadRequest(new { errorMessage });
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

                return BadRequest(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return BadRequest(new { errorMessage });
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
                var conversionedDt = GetConvertAuthorizedKubunFromNumberToString(dt);
                // メール受け取り要否列をboolから文字に変換
                conversionedDt = GetConvertedIsRequiredMailFromBoolToString(conversionedDt);

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

                        CreateFile.DeleteFile(tmpFilename);

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
        private DataTable GetConvertAuthorizedKubunFromNumberToString(DataTable dt)
        {
            var index = dt.Columns.IndexOf("authorized_kubun");
            dt.Columns.Add("authorized_kubun_name").SetOrdinal(index);
            foreach (DataRow row in dt.Rows)
            {
                var authorizedKubun = (int)row["authorized_kubun"];
                if (authorizedKubun == 0)
                {
                    row["authorized_kubun_name"] = "なし";
                }
                else if (authorizedKubun == 1)
                {
                    row["authorized_kubun_name"] = "管理者";
                }
            }
            dt.Columns.Remove("authorized_kubun");

            return dt;
        }

        /// <summary>
        /// ADに入力された名前が存在するか
        /// </summary>
        /// <param name="userName">ユーザー名</param>
        /// <returns>認証されたユーザー名</returns>
        private bool HasNameInAD(string userName)
        {
            try
            {
                string ldapPath = "LDAP://192.168.1.6/DC=tozan,DC=co,DC=jp";
                DirectoryEntry directoryEntry = new DirectoryEntry();
                directoryEntry.Path = ldapPath;

                // Active Directory でユーザーを検索
                DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                searcher.Filter = "(&(objectClass=user)(sAMAccountName=" + userName + "))";
                searcher.SearchScope = SearchScope.Subtree;

                // ユーザーが見つかったかどうかを確認
                SearchResult result = searcher.FindOne();

                if (result == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 各種入力チェック
        private ValidCheckModel ValidCheck(M_UserModel model)
        {
            // 入力規則チェック
            if (!ModelState.IsValid)
                return new ValidCheckModel{
                    IsValid = false,
                    ErrorMessage = "E1011: " + ErrorMessagesResources.E1011
                };

            // ユーザー名重複チェック
            if (IsADNameDuplicate(model))
            {
                string displayName = Utils.GetDisplayName<M_UserModel>("ADName");
                return new ValidCheckModel
                {
                    IsValid = false,
                    ErrorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, displayName),
                };
            }

            // メールの入力チェック
            var mailValid = MailCheck(model);
            if (!mailValid.IsValid)
                return new ValidCheckModel
                {
                    IsValid = false,
                    ErrorMessage = mailValid.ErrorMessage,
                };


            return new ValidCheckModel
            {
                IsValid = true,
                ErrorMessage = "",
            };
        }

        // メールの入力チェック
        private ValidCheckModel MailCheck(M_UserModel model)
        {

            // メールアドレスに入力があり、
            // かつメールアドレスの形式ではない
            if (!string.IsNullOrEmpty(model.MailAddress) && !IsValidMailAddress(model.MailAddress))
                return new ValidCheckModel()
                {
                    IsValid = false,
                    ErrorMessage = "E1011: " + ErrorMessagesResources.E1011
                };

            // メールを受け取る
            // かつメールアドレスの入力がない
            if (model.IsRequiredMail && string.IsNullOrEmpty(model.MailAddress))
                return new ValidCheckModel()
                {
                    IsValid = false,
                    ErrorMessage = "E1014: " + ErrorMessagesResources.E1014
                };

            return new ValidCheckModel()
            {
                IsValid = true,
                ErrorMessage = ""
            };
        }

        /// <summary>
        /// 指定された文字列がメールアドレスとして正しい形式か検証する
        /// </summary>
        /// <param name="address">検証する文字列</param>
        /// <returns>正しい時はTrue。正しくない時はFalse。</returns>
        private bool IsValidMailAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            try
            {
                System.Net.Mail.MailAddress a =
                    new System.Net.Mail.MailAddress(address);
            }
            catch (FormatException)
            {
                //FormatExceptionがスローされた時は、正しくない
                return false;
            }

            return true;
        }

        private DataTable GetConvertedIsRequiredMailFromBoolToString(DataTable dt)
        {
            // テーブルに値を変換した後の文字列を格納する列を追加
            dt.Columns.Add("converted_is_required_mail", typeof(string)).SetOrdinal(dt.Columns.IndexOf("is_required_mail"));
            // 各列の値を適切な値に変換
            foreach (DataRow row in dt.Rows)
            {
                if (row["is_required_mail"].ToString() == "True")
                {
                    row["converted_is_required_mail"] = "受け取る";
                }
                else
                {
                    row["converted_is_required_mail"] = "受け取らない";
                }
            }

            // 変換前の列を削除
            dt.Columns.Remove("is_required_mail");
            return dt;
        }
    }
}
