using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace mar_sumaken_web.Controllers
{
    public class M_ProductController : BaseController
    {
        private readonly ILogger<M_ProductController> _logger;

        public M_ProductController(ILogger<M_ProductController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 商品マスター情報取得
        /// </summary>
        public IActionResult Index()
        {
            M_ProductModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View();
                }

                // 商品マスター情報取得SQL作成
                var sql = M_ProductConnectController.CreateSQLToSelectMProducts();
                // DB接続
                List<M_ProductModel> productList = M_ProductConnectController.ConnectMProducts(sql, user.DatabaseName);
                if (productList.Count > 0)
                {
                    // 倉庫-品番中間取得
                    productList = M_ProductConnectController.GetRDepoProducts(productList, user.DatabaseName);
                }

                // ビューのタイトル取得
                model = new()
                {
                    ControllerName = "M_Product",
                    CompanyID = user.CompanyID,
                    MProductList = productList
                };
                // 倉庫リスト
                model.RDepoProductsRegister = (List<SelectListItem>)model.GetMDepoList(user.DatabaseName);
                // 仕入先リストを取得
                model.SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_Supplier_ID, user.DatabaseName);
                // 納入先リストを取得
                model.DeliverySelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_Delivery_ID, user.DatabaseName);

                ViewData["Title"] = model.GetViewTitle();
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 品番マスターを削除
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns></returns>
        public IActionResult Delete(int productId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || productId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 品番マスター削除
                bool isDeletedProduct = M_ProductConnectController.DeleteMProduct(productId, user.DatabaseName);

                // 更新件数が0の場合はエラーとする
                if (!isDeletedProduct)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                }

                return Ok();
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 品番マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_ProductModel model = new M_ProductModel();
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

                // 倉庫リスト
                model.RDepoProductsRegister = (List<SelectListItem>)model.GetMDepoList(user.DatabaseName);

                // 仕入先リストを取得
                model.SuplierSelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_Supplier_ID, user.DatabaseName);

                // 納入先リストを取得
                model.DeliverySelectList = M_ProductConnectController.GetCompanysByCompanyKubun(Utils.Const_Delivery_ID, user.DatabaseName);

                return View(model);
            }
            catch (Exception)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
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
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null || model.RDepoProductsRegister == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 使用倉庫をチェック
                bool isSelectedDepo = model.RDepoProductsRegister.Any(item => item.Selected);
                if (!isSelectedDepo)
                {
                    ModelState.AddModelError("RDepoProductsRegister", ErrorMessagesResources.E1001); 
                }

                // 登録情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "正しい入力値を入力してください。" });
                }

                // 重複品番情報をチェック
                bool isDuplicate = M_ProductConnectController.IsDuplicateMProduct(model, user.DatabaseName);
                if (isDuplicate)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return NotFound(new { errorMessage = "登録品番情報が重複しています。" });
                }

                // 品番マスター登録
                bool isInserted = M_ProductConnectController.InsertMProduct(model, user);

                // 更新件数が0の場合はエラーとする
                if (!isInserted)
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
        /// 品番マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_ProductModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null || model.RDepoProductsRegister == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 使用倉庫をチェック
                bool isSelectedDepo = model.RDepoProductsRegister.Any(item => item.Selected);
                if (!isSelectedDepo)
                {
                    ModelState.AddModelError("RDepoProductsRegister", ErrorMessagesResources.E1001);
                }

                // 更新情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "正しい入力値を入力してください。" });
                }

                // 重複品番更新情報をチェック
                bool isDuplicate = M_ProductConnectController.IsDuplicateEditMProduct(model, user.DatabaseName);
                if (isDuplicate)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
                    return NotFound(new { errorMessage = "更新品番情報が重複しています。" });
                }

                // 品番マスター更新
                bool updatedFlg = M_ProductConnectController.UpdateMProduct(model, user);

                // 更新件数が0の場合はエラーとする
                if (!updatedFlg)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "更新はできませんでした。" });
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

                // 品番マスター情報取得SQL作成
                var sql = M_ProductConnectController.CreateSQLToSelectMProducts();
                // DB接続
                List<M_ProductModel> selectedList = M_ProductConnectController.ConnectMProducts(sql, user.DatabaseName);
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
                var tmpFilename = "品番マスター.csv";
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
                return Json(new { res = "NG", error = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// 品番マスターテーブルを作る
        /// </summary>
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
