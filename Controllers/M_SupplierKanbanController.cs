using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 仕入先かんばんマスター画面
    /// </summary>
    public class M_SupplierKanbanController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    var errormsgs = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    // log取得
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"仕入先かんばんマスター登録失敗 {errorMessage} {errormsgs}");

                    return NotFound(new { errorMessage });
                }

                // 仕入先かんばんコード重複チェック
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectDuplicateMSupplierKanban(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_SupplierKanbanModel>("DepoName") + "・" + Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyString") + "・" + Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyStringStartIndex"));
                    _logger.Error($"仕入先かんばんマスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 仕入先かんばんマスター登録
                M_SupplierKanbanConnectController.InsertMSupplierKanban(model, user);

                // log取得
                _logger.Info($"仕入先かんばんマスター登録成功 仕入先かんばん名:{model.SupplierKanbanName}");

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
        /// 仕入先かんばんマスター更新画面表示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            M_SupplierKanbanModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先かんばんマスター情報取得
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectMSupplierKanbans(id);
                var supplierKanbanList = M_SupplierKanbanConnectController.ConnectMSupplierKanbans(sql, user.DatabaseName);

                if (supplierKanbanList.Count == 1)
                {
                    // ユーザーマスターの詳細を取得
                    IEnumerable<M_SupplierKanbanModel> mSupplierKanbanList = M_SupplierKanbanConnectController.GetMSupplierKanbanDetailList(supplierKanbanList, user.DatabaseName);
                    model = mSupplierKanbanList.FirstOrDefault();
                }

                // 会社マスター情報取得
                model.SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_SupplierID, user.DatabaseName);
                model.SelectedSupplierID = model.SupplierID;

                // 倉庫
                model.SelectedDepoID = model.DepoID;

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

                // 使用ハンディメニュー名
                foreach (var item in model.M_HandyMenuList)
                {
                    foreach (var menuItem in model.HandyMenuSelectList)
                    {
                        if (item.HandyMenuID.ToString().Equals(menuItem.Value))
                        {
                            menuItem.Selected = true;
                        }
                    }
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
        /// 仕入先かんばんマスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_SupplierKanbanModel model)
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
                    _logger.Error($"仕入先かんばんマスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 仕入先かんばんマスター更新
                M_SupplierKanbanConnectController.UpdateMSupplierKanban(model, user);

                // log取得
                _logger.Info($"仕入先かんばんマスター更新成功 仕入先かんばんID:{model.SupplierKanbanID}");

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
        /// 仕入先かんばんマスター削除
        /// </summary>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <returns></returns>
        public IActionResult Delete(int supplierKanbanId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先かんばんマスター削除
                M_SupplierKanbanConnectController.DeleteMSupplierKanban(supplierKanbanId, user);

                // log取得
                _logger.Info($"仕入先かんばんマスター削除成功 会社ID:{supplierKanbanId}");

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
        /// <param name="searchModel"></param>
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

                if(matchSupplierKanban != null)
                {
                    model = matchSupplierKanban;
                    model.QRCodeString = qrCodeString;
                    return Ok(model);
                }
                else
                {
                    return NotFound(new { errorMessage = "E1022: " + ErrorMessagesResources.E1022 });
                }
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
        /// QRコードの部分文字列
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stringLength"></param>
        /// <param name="qr"></param>
        /// <returns>抜き出した文字列</returns>
        public static string QrcodeValueSubstring(int index, int stringLength, string qr)
        {
            var substr = "";

            // 文字列を取得するための最小桁数
            int minLength = index - 1 + stringLength;
            if (qr.Length >= minLength)
            {
                substr = qr.Substring(index - 1, stringLength).Replace(" ", "");
            }
            return substr;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">画面名</param>
        public JsonResult ExportFile(string gamenName)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // テーブルデータ取得
                DataTable mSupplierKanbanDataTable = CreateDataTable();

                // 仕入先マスター情報取得
                var sql = M_SupplierKanbanConnectController.CreateSQLToSelectMSupplierKanbans();
                List<M_SupplierKanbanModel> supplierKanbanList = M_SupplierKanbanConnectController.ConnectMSupplierKanbans(sql, user.DatabaseName);

                // DataRowに格納
                if (supplierKanbanList.Count > 0)
                {
                    foreach (M_SupplierKanbanModel supplierKanbanItem in supplierKanbanList)
                    {
                        DataRow newRow = mSupplierKanbanDataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanID")] = supplierKanbanItem.SupplierKanbanID.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("DepoName")] = supplierKanbanItem.DepoName;
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierName")] = supplierKanbanItem.SupplierName;
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanName")] = supplierKanbanItem.SupplierKanbanName;
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("AllowedDuplicatesFlag")] = supplierKanbanItem.AllowedDuplicatesFlag.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyString")] = supplierKanbanItem.IdentifyString.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyStringStartIndex")] = supplierKanbanItem.IdentifyStringStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("ProductNumberLength")] = supplierKanbanItem.ProductNumberLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("ProductNumberStartIndex")] = supplierKanbanItem.ProductNumberStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("QuantityLength")] = supplierKanbanItem.QuantityLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("QuantityStartIndex")] = supplierKanbanItem.QuantityStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("LotLength")] = supplierKanbanItem.LotLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("LotStartIndex")] = supplierKanbanItem.LotStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("MainProductKeyLength")] = supplierKanbanItem.MainProductKeyLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("MainProductKeyStartIndex")] = supplierKanbanItem.MainProductKeyStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("FirstSubProductKeyLength")] = supplierKanbanItem.FirstSubProductKeyLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("FirstSubProductKeyStartIndex")] = supplierKanbanItem.FirstSubProductKeyStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SecondSubProductKeyLength")] = supplierKanbanItem.SecondSubProductKeyLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("SecondSubProductKeyStartIndex")] = supplierKanbanItem.SecondSubProductKeyStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("ProductBranchNumberLength")] = supplierKanbanItem.ProductBranchNumberLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("ProductBranchNumberStartIndex")] = supplierKanbanItem.ProductBranchNumberStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("OrderNumberLength")] = supplierKanbanItem.OrderNumberLength.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("OrderNumberStartIndex")] = supplierKanbanItem.OrderNumberStartIndex.ToString();
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedAt")] = supplierKanbanItem.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedBy")] = supplierKanbanItem.UpdatedBy;

                        mSupplierKanbanDataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(null, gamenName);
                // CSVファイルへのパスを作成
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換
                CreateFile.ConvertDataTableToCsv(mSupplierKanbanDataTable, filePath);
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

            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("DepoName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SupplierKanbanName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("AllowedDuplicatesFlag"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyString"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("IdentifyStringStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("ProductNumberLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("ProductNumberStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("QuantityLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("QuantityStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("LotLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("LotStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("MainProductKeyLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("MainProductKeyStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("FirstSubProductKeyLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("FirstSubProductKeyStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SecondSubProductKeyLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("SecondSubProductKeyStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("ProductBranchNumberLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("ProductBranchNumberStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("OrderNumberLength"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("OrderNumberStartIndex"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_SupplierKanbanModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
