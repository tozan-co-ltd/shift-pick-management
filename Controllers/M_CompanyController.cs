using mar_sumaken_web.Commons;
using Microsoft.AspNetCore.Mvc;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using System.Data;

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
        /// 会社マスターリスト取得
        /// </summary>
        public IActionResult Index()
        {
            List<M_CompanyModel> listCompany = new List<M_CompanyModel>();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View(listCompany);
                }

                // 会社マスター情報取得SQL作成
                var sql = M_CompanyConnectController.CreateSQLToSelectMCompanys();
                // DB接続
                List<M_CompanyModel> companyList = M_CompanyConnectController.ConnectMCompanys(sql, user.DatabaseName);
                if (companyList.Count > 0)
                {
                    listCompany = companyList;
                }

                return View(listCompany);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(listCompany);
            }
        }

        /// <summary>
        /// 会社マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_CompanyModel model = new M_CompanyModel();
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
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
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
                    return NotFound(new { errorMessage = "正しい入力値を入力してください。" });
                }

                // 重複会社情報取をチェック
                bool isDuplicate = M_CompanyConnectController.IsDuplicateMCompanyByCompanyCode(model.CompanyCode, user.DatabaseName);
                if (isDuplicate)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return NotFound(new { errorMessage = "会社コードが重複しています。" });
                }

                // 会社マスター登録
                int insertedCount = M_CompanyConnectController.InsertMCompany(model, user);

                // 更新件数が0の場合はエラーとする
                if (insertedCount == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "登録はできませんでした。" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 会社マスターを削除
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

                // 更新件数が0の場合はエラーとする
                if (deleteAffectedRows == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
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
                        newRow["会社ID"] = item.CompanyID.ToString();
                        newRow["会社コード"] = item.CompanyCode.ToString();
                        newRow["会社区分"] = item.CompanyKubunName;
                        newRow["会社名"] = item.CompanyName;
                        newRow["取引先名"] = item.ClientName;
                        newRow["更新日時"] = item.UpdatedAt.ToString();
                        newRow["更新者"] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = "会社マスター.csv";
                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換する
                Utils.ToCSV(dataTable, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception ex)
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
        private DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("会社ID", typeof(string));
            table.Columns.Add("会社コード", typeof(string));
            table.Columns.Add("会社区分", typeof(string));
            table.Columns.Add("会社名", typeof(string));
            table.Columns.Add("取引先名", typeof(string));
            table.Columns.Add("更新日時", typeof(string));
            table.Columns.Add("更新者", typeof(string));
            return table;
        }
    }
}
