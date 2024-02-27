using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 倉庫マスター画面
    /// </summary>
    public class M_DepoController : BaseController
    {
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

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }

                // 倉庫マスター情報取得SQL作成
                var sql = M_DepoConnectController.CreateSQLToSelectMDepos();
                // DB接続
                IEnumerable<M_DepoModel> demoList = M_DepoConnectController.ConnectMDepos(sql, user.DatabaseName);

                model.M_DepoList = demoList.ToPagedList();

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
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
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }

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
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage = errorMessages });
                }

                // 倉庫コード重複チェック
                var sql = M_DepoConnectController.CreateSQLToSelectDuplicateMDepo(model.DepoCode);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    return NotFound(new { errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_DepoModel>("DepoCode")) });
                }

                // 倉庫マスター登録
                M_DepoConnectController.InsertMDepo(model, user);

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

        /// <summary>
        /// 倉庫マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_DepoModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage = errorMessages });
                }

                // 異なるIDで倉庫コード重複チェック
                var sql = M_DepoConnectController.CreateSQLToSelectDuplicateEditMDepo(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    return NotFound(new { errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_DepoModel>("DepoCode")) });
                }

                // 倉庫マスター更新
                M_DepoConnectController.UpdateMDepo(model, user);

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

        /// <summary>
        /// 倉庫マスター削除
        /// </summary>
        /// <param name="depoId">倉庫ID</param>
        /// <returns></returns>
        public IActionResult Delete(int depoId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター削除
                int deleteAffectedRows = M_DepoConnectController.DeleteMDepo(depoId, user) ;

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

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <param name="gamenName">画面名</param>
        public JsonResult ExportCsv(SearchConditionModel searchModel, string gamenName)
        {
            try
            {
                // DataTable作成
                DataTable dataTable = CreateDataTable();

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 検索情報取得
                var sql = M_DepoConnectController.CreateSQLToSelectMDepos();
                List<M_DepoModel> searchList = M_DepoConnectController.ConnectMDepos(sql, user.DatabaseName);
                if (searchList.Count > 0)
                {
                    foreach (M_DepoModel item in searchList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoID")] = item.DepoID.ToString();
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoCode")] = item.DepoCode;
                        newRow[Utils.GetDisplayName<M_DepoModel>("DepoName")] = item.DepoName;
                        newRow[Utils.GetDisplayName<M_DepoModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss");
                        newRow[Utils.GetDisplayName<M_DepoModel>("UpdatedBy")] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名作成
                string fileName = CreateFileController.CreateFileName(searchModel, gamenName);

                // CSVファイルパス作成
                string filePath = Path.Combine(Path.GetTempPath(), fileName);

                // DataTableをCSV形式の文字列に変換
                CreateFileController.ConvertDataTableToCsv(dataTable, filePath);

                // ファイル作成
                var fileResult = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(fileResult, System.Net.Mime.MediaTypeNames.Application.Octet, fileName) });
            }
            catch (Exception ex)
            {
                return Json(new { errorMessage = ex.Message });
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
