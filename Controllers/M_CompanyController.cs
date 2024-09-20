using  ai_truck_load_measurement.Commons;
using  ai_truck_load_measurement.ConnectControllers;
using  ai_truck_load_measurement.Models;
using  ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using X.PagedList;

namespace  ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 会社マスター画面
    /// </summary>
    public class M_CompanyController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 会社マスター画面表示
        /// </summary>
        public IActionResult Index()
        {
            M_CompanyModel model = new M_CompanyModel();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社マスター情報取得SQL作成
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();
                // DB接続
                IEnumerable<M_CompanyModel> companyList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);

                model.M_CompanyList = companyList.ToPagedList();

                // 会社区分リスト取得
                model.KubunSelectList = Utils.Const_CompanyKubunList;

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
        /// 会社マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_CompanyModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社区分リスト取得
                model.KubunSelectList = Utils.Const_CompanyKubunList;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 会社マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_CompanyModel model)
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
                    _logger.Error($"会社マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 会社コード重複チェック
                var sql = M_CompanyConnectController.CreateSQLToSelectDuplicateMCompany(model.CompanyCode);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_CompanyModel>("CompanyCode"));
                    _logger.Error($"会社マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 会社マスター登録
                M_CompanyConnectController.InsertMCompany(model, user);

                // log取得
                _logger.Info($"会社マスター登録成功 会社コード:{model.CompanyCode}");

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
        /// 会社マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_CompanyModel model)
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
                    _logger.Error($"会社マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 異なるIDで会社コード重複チェック
                var sql = M_CompanyConnectController.CreateSQLToSelectDuplicateEditMCompany(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_CompanyModel>("CompanyCode"));
                    _logger.Error($"会社マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 会社マスター更新
                M_CompanyConnectController.UpdateMCompany(model, user);

                // log取得
                _logger.Info($"会社マスター更新成功 会社ID:{model.CompanyID}");

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
        /// 会社マスター削除
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <returns></returns>
        public IActionResult Delete(int companyId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社マスター削除
                M_CompanyConnectController.DeleteMCompany(companyId, user);

                // log取得
                _logger.Info($"会社マスター削除成功 会社ID:{companyId}");

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

                // 会社マスター情報取得
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();
                List<M_CompanyModel> selectedList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);

                // DataRowに格納
                if (selectedList.Count > 0)
                {
                    foreach (M_CompanyModel item in selectedList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_CompanyModel>("CompanyID")] = item.CompanyID.ToString();
                        newRow[Utils.GetDisplayName<M_CompanyModel>("CompanyCode")] = item.CompanyCode.ToString();
                        newRow[Utils.GetDisplayName<M_CompanyModel>("CompanyKubun")] = item.CompanyKubunName;
                        newRow[Utils.GetDisplayName<M_CompanyModel>("CompanyName")] = item.CompanyName;
                        newRow[Utils.GetDisplayName<M_CompanyModel>("ClientName")] = item.ClientName;
                        newRow[Utils.GetDisplayName<M_CompanyModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<M_CompanyModel>("UpdatedBy")] = item.UpdatedBy;

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
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("CompanyID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("CompanyCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("CompanyKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("CompanyName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("ClientName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_CompanyModel>("UpdatedBy"), typeof(string));
            return table;
        }
    }
}
