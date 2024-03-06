using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 仕入先かんばんマスター画面
    /// </summary>
    public class M_SupplierKanbanController : BaseController
    {
        /// <summary>
        /// 仕入先かんばんマスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_SupplierKanbanModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先かんばんマスター情報取得
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectMSupplierKanbans();
                var supplierKanbanList = M_SupplierKanbanConnectController.ConnectMSupplierKanbans(sql, user.DatabaseName);

                if (supplierKanbanList.Count > 0)
                {
                    // ユーザーマスターの詳細を取得
                    IEnumerable<M_SupplierKanbanModel> mSupplierKanbanList = M_SupplierKanbanConnectController.GetMSupplierKanbanDetailList(supplierKanbanList, user.DatabaseName);
                    model.M_SupplierKanbanList = mSupplierKanbanList.ToPagedList();
                }
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
        /// 仕入先かんばんマスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_SupplierKanbanModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // ハンディメニューマスター情報取得
                var handyMenuListSql = M_HandyMenuConnectController.CreateSQLToSelectMHandyMenuList();
                List<M_HandyMenuModel> handyMenuList = M_HandyMenuConnectController.ConnectMHandyMenus(handyMenuListSql, user.DatabaseName);
                foreach (var handyMenu in handyMenuList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = handyMenu.HandyMenuName,
                        Value = Convert.ToString(handyMenu.HandyMenuID),
                        Selected = false
                    };

                    model.HandyMenuSelectList.Add(menuItem);
                }

                // 会社マスター情報取得
                model.SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_SupplierID, user.DatabaseName);

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 仕入先かんばんマスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_SupplierKanbanModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // メインキーチェック
                // 重複許容フラグ=0の場合はメインキー必須


                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 仕入先かんばんコード重複チェック
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectDuplicateMSupplierKanban(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    return NotFound(new { errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_SupplierKanbanModel>("DepoName") + "・" + Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyString") + "・" + Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyStringStartIndex")) });
                }

                // 仕入先かんばんマスター登録
                M_SupplierKanbanConnectController.InsertMSupplierKanban(model, user);

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
        /// 仕入先かんばんマスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_SupplierKanbanModel model)
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

                // 異なるIDで仕入先かんばんコード重複チェック
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectDuplicateEditMSupplierKanban(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    return NotFound(new { errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyString") + "・" +  Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyStringStartIndex")) });
                }

                // 仕入先かんばんマスター更新
                M_SupplierKanbanConnectController.UpdateMSupplierKanban(model, user);

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
        /// 仕入先かんばんマスター削除
        /// </summary>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <returns></returns>
        public IActionResult Delete(int supplierKanbanId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先かんばんマスター削除
                M_SupplierKanbanConnectController.DeleteMSupplierKanban(supplierKanbanId, user) ;

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
        /// 仕入先かんばんマスタープレビュー画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Preview()
        {
            M_SupplierKanbanModel model = new();
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
        /// プレビュー結果取得
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PreviewResult(M_SupplierKanbanModel searchModel)
        {
            M_SupplierKanbanModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先かんばんマスター情報取得
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectMSupplierKanbans();
                var supplierKanbanList = M_SupplierKanbanConnectController.ConnectMSupplierKanbans(sql, user.DatabaseName);

                // QRコード文字列チェック
                string qrCodeString = searchModel.QRCodeString;
                // 仕入先かんばんマスターの識別文字・識別文字開始位置でトリムし、一致するレコードを取得
                var matchSupplierKanban = supplierKanbanList.Where(x => x.IdentifyString == QrcodeValueSubstring(x.IdentifyStringStartIndex, x.IdentifyString.Length, qrCodeString)).ToList().FirstOrDefault();

                model = matchSupplierKanban;
                model.QRCodeString = qrCodeString;

                return Ok(model);
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
        /// Qrコードの部分文字列
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stringLength"></param>
        /// <param name="qr"></param>
        /// <returns></returns>
        public static string QrcodeValueSubstring(int index, int stringLength, string qr)
        {
            var value = qr.Substring(index - 1, stringLength).Replace(" ", "");
            return value;
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
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectMSupplierKanbans();
                List<M_SupplierKanbanModel> searchList = M_SupplierKanbanConnectController.ConnectMSupplierKanbans(sql, user.DatabaseName);
                if (searchList.Count > 0)
                {
                    foreach (M_SupplierKanbanModel item in searchList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanID")] = item.SupplierKanbanID.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss");
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedBy")] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名作成
                string fileName = CreateFile.CreateFileName(searchModel, gamenName);

                // CSVファイルパス作成
                string filePath = Path.Combine(Path.GetTempPath(), fileName);

                // DataTableをCSV形式の文字列に変換
                CreateFile.ConvertDataTableToCsv(dataTable, filePath);

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

            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
