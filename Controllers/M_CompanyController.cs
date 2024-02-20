using mar_sumaken_web.Commons;
using Microsoft.AspNetCore.Mvc;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using System.Data;
using System.Reflection;
using X.PagedList;
using System.Data.SqlClient;

namespace mar_sumaken_web.Controllers
{
    public class M_CompanyController : BaseController
    {
        private readonly ILogger<M_CompanyController> _logger;

        public M_CompanyController(ILogger<M_CompanyController> logger)
        {
            _logger = logger;
        }

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

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View(model);
                }

                // 会社マスター情報取得SQL作成
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();

                // DB接続
                IEnumerable<M_CompanyModel> companyList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);

                model.M_CompanyList = companyList.ToPagedList();

                // 会社区分リスト取得
                model.KubunSelectList = Utils.Const_Company_Kubun_List;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
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

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View(model);
                }

                // 会社区分リスト取得
                model.KubunSelectList = Utils.Const_Company_Kubun_List;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
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
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。"});
                }

                // 登録情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "" });
                }

                // 重複会社情報取をチェック
                bool isDuplicate = M_CompanyConnectController.IsDuplicateMCompanyByCompanyCode(model.CompanyCode, user.DatabaseName);
                if (isDuplicate)
                {
                    return NotFound(new { errorMessage = "会社コードが重複しています。" });
                }

                // 会社マスター登録
                int insertedCount = M_CompanyConnectController.InsertMCompany(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E4001 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 会社マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_CompanyModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 更新情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "" });
                }

                // 重複会社情報取をチェック
                bool isDuplicate = M_CompanyConnectController.IsDuplicateEditMCompanyByCompanyCode(model.CompanyCode, model.CompanyID, user.DatabaseName);
                if (isDuplicate)
                {
                    return NotFound(new { errorMessage = "会社コードが重複しています。" });
                }

                // 会社マスター更新
                int editedCount = M_CompanyConnectController.EditMCompany(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E4001 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 会社マスター削除
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <returns></returns>
        public IActionResult Delete(int companyId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || companyId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 会社マスター削除
                int deleteAffectedRows = M_CompanyConnectController.DeleteMCompany(companyId, user.DatabaseName);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E4001 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        public JsonResult ExportFile()
        {
            string? errorMessage;
            try
            {
                // log取得
                _logger.LogInformation($"Excel出力開始");

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    throw new Exception();
                }

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // 会社マスター情報取得SQL作成
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();
                // DB接続
                List<M_CompanyModel> selectedList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);
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
                        newRow[Utils.GetDisplayName<M_CompanyModel>("UpdatedAt")] = item.UpdatedAt.ToString();
                        newRow[Utils.GetDisplayName<M_CompanyModel>("UpdatedBy")] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = "会社マスター.csv";
                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換する
                ReadFile.ToCSV(dataTable, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogInformation($"{exceptionMessage} {errorMessage}");
                return Json(new { res = "NG", error = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// 会社マスターテーブルを作る
        /// </summary>
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
