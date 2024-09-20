using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 倉庫マスター画面
    /// </summary>
    public class M_DepoController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 倉庫マスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_DepoModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター情報取得SQL作成
                var sql = M_DepoConnectController.CreateSQLToSelectMDepos();
                // DB接続
                IEnumerable<M_DepoModel> demoList = M_DepoConnectController.ConnectMDepos(sql, user.DatabaseName);

                model.M_DepoList = demoList.ToPagedList();

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
        /// 倉庫マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_DepoModel model = new();
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
        /// 倉庫マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_DepoModel model)
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
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"倉庫マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 倉庫コード重複チェック
                var sql = M_DepoConnectController.CreateSQLToSelectDuplicateMDepo(model.DepoCode);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_DepoModel>("DepoCode"));
                    _logger.Error($"倉庫マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 倉庫マスター登録
                M_DepoConnectController.InsertMDepo(model, user);

                // log取得
                _logger.Info($"倉庫マスター登録成功 倉庫コード:{model.DepoCode}");

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
        /// 倉庫マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_DepoModel model)
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
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"倉庫マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 異なるIDで倉庫コード重複チェック
                var sql = M_DepoConnectController.CreateSQLToSelectDuplicateEditMDepo(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_DepoModel>("DepoCode"));
                    _logger.Error($"倉庫マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 倉庫マスター更新
                M_DepoConnectController.UpdateMDepo(model, user);

                // log取得
                _logger.Info($"倉庫マスター更新成功 倉庫ID:{model.DepoID}");

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
        /// 倉庫マスター削除
        /// </summary>
        /// <param name="depoId">倉庫ID</param>
        /// <returns></returns>
        public IActionResult Delete(int depoId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター削除
                int deleteAffectedRows = M_DepoConnectController.DeleteMDepo(depoId, user) ;

                // log取得
                _logger.Info($"倉庫マスター削除成功 倉庫ID:{depoId}");

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
        /// <param name="gamenName">画面名</param>
        public JsonResult ExportFile(string gamenName)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // 倉庫マスター情報取得
                var sql = M_DepoConnectController.CreateSQLToSelectMDepos();
                List<M_DepoModel> selectedList = M_DepoConnectController.ConnectMDepos(sql, user.DatabaseName);

                // DataRowに格納
                if (selectedList.Count > 0)
                {
                    foreach (M_DepoModel item in selectedList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoID")] = item.DepoID.ToString();
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoCode")] = item.DepoCode;
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoName")] = item.DepoName;
                        newRow[Utils.GetDisplayName<M_DepoModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<M_DepoModel>("UpdatedBy")] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(null, gamenName);
                // CSVファイルへのパスを作成
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換
                CreateFile.ConvertDataTableToCsv(dataTable, filePath);
                // ファイル作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (SqlException)
            {
                return Json(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception ex)
            {
                return Json(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 + ex.Message });
            }
        }

        /// <summary>
        /// データテーブル作成
        /// </summary>
        /// <returns></returns>
        private static DataTable CreateDataTable()
        {
            var table = new DataTable();

            table.Columns.Add(Utils.GetDisplayName<M_DepoModel>("DepoID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_DepoModel>("DepoCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_DepoModel>("DepoName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_DepoModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_DepoModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
