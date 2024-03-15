using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 品番マスター画面
    /// </summary>
    public class M_ProductController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 品番マスター画面表示
        /// </summary>
        public IActionResult Index()
        {
            M_ProductModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 品番マスター情報取得SQL作成
                var sql = M_ProductConnectController.CreateSQLToSelectMProducts();
                // DB接続
                List<M_ProductModel> productList = M_ProductConnectController.ConnectMProducts(sql, user.DatabaseName);
                if (productList.Count > 0)
                {
                    // 倉庫-品番中間テーブル情報取得
                    productList = M_ProductConnectController.GetRDepoProducts(productList, user.DatabaseName);
                }

                // 表示データをModelに格納
                model = new()
                {
                    MProductList = productList,
                    RDepoProductsRegister = (List<SelectListItem>)model.GetMDepoList(user.DatabaseName),
                    SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_SupplierID, user.DatabaseName),
                    DeliverySelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_DeliveryID, user.DatabaseName),
                };
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
        /// 品番マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_ProductModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 表示データをModelに格納
                model = new()
                {
                    RDepoProductsRegister = (List<SelectListItem>)model.GetMDepoList(user.DatabaseName),
                    SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_SupplierID, user.DatabaseName),
                    DeliverySelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_DeliveryID, user.DatabaseName),

                };

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 品番マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_ProductModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 使用倉庫名の選択チェック
                bool isSelectedDepo = model.RDepoProductsRegister.Any(item => item.Selected);
                if (!isSelectedDepo)
                {
                    ModelState.AddModelError("RDepoProductsRegister", "E1001: " + string.Format(ErrorMessagesResources.E1001, Utils.GetDisplayName<M_ProductModel>("RDepoProductsRegister"))); 
                }

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"品番マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 仕入先品番・納入先品番重複チェック
                var sql = M_ProductConnectController.CreateSQLToSelectDuplicateMProduct(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_ProductModel>("SupplierProductNumber") + "または" + Utils.GetDisplayName<M_ProductModel>("DeliveryProductNumber"));
                    _logger.Error($"品番マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 品番マスター登録
                M_ProductConnectController.InsertMProduct(model, user);

                // log取得
                _logger.Info($"品番マスター登録成功 仕入先品番:{model.SupplierProductNumber}");

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
        /// 品番マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_ProductModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 使用倉庫選択チェック
                bool isSelectedDepo = model.RDepoProductsRegister.Any(item => item.Selected);
                if (!isSelectedDepo)
                {
                    ModelState.AddModelError("RDepoProductsRegister", "E1001: " + ErrorMessagesResources.E1001);
                }

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"品番マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 異なるIDで仕入先品番・納入先品番重複チェック
                var sql = M_ProductConnectController.CreateSQLToSelectDuplicateEditMProduct(model);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_ProductModel>("SupplierProductNumber") + "または" + Utils.GetDisplayName<M_ProductModel>("DeliveryProductNumber"));
                    _logger.Error($"品番マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 品番マスター更新
                M_ProductConnectController.UpdateMProduct(model, user);

                // log取得
                _logger.Info($"品番マスター更新成功 品番ID:{model.ProductID}");

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
        /// 品番マスター削除
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns></returns>
        public IActionResult Delete(int productId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 品番マスター削除
                M_ProductConnectController.DeleteMProduct(productId, user);

                // log取得
                _logger.Info($"品番マスター削除成功 品番ID:{productId}");

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
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // 品番マスター情報取得
                var sql = M_ProductConnectController.CreateSQLToSelectMProducts();
                List<M_ProductModel> selectedList = M_ProductConnectController.ConnectMProducts(sql, user.DatabaseName);

                // DataRowに格納
                if (selectedList.Count > 0)
                {
                    foreach (M_ProductModel item in selectedList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_ProductModel>("ProductID")] = item.ProductID.ToString();
                        newRow[Utils.GetDisplayName<M_ProductModel>("SupplierName")] = item.SupplierName;
                        newRow[Utils.GetDisplayName<M_ProductModel>("SupplierProductNumber")] = item.SupplierProductNumber;
                        newRow[Utils.GetDisplayName<M_ProductModel>("DeliveryName")] = item.DeliveryName;
                        newRow[Utils.GetDisplayName<M_ProductModel>("DeliveryProductNumber")] = item.DeliveryProductNumber;
                        newRow[Utils.GetDisplayName<M_ProductModel>("ProductName")] = item.ProductName;
                        newRow[Utils.GetDisplayName<M_ProductModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<M_ProductModel>("RDepoProductNames")] = item.RDepoProductNames;
                        newRow[Utils.GetDisplayName<M_ProductModel>("UpdatedAt")] = item.UpdatedAt.ToString();
                        newRow[Utils.GetDisplayName<M_ProductModel>("UpdatedBy")] = item.UpdatedBy;

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
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("ProductID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("DeliveryName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("DeliveryProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("ProductName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("LotQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("RDepoProductNames"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_ProductModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
