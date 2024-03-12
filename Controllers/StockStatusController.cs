using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 在庫照会画面
    /// </summary>
    public class StockStatusController : BaseController
    {
        /// <summary>
        /// 在庫照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            StockStatusModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社リスト取得
                CommonModel commonModel = new();
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_SupplierID);

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
        /// 在庫照会情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(StockStatusModel searchModel)
        {
            StockStatusModel model = new();
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 在庫情報取得
                var sql = StockStatusConnectController.CreateSQLToGetStockStatus(
                    searchModel.DateSearchStart, searchModel.SelectedDepoID, searchModel.SelectedCompanyID);
                List<StockStatusModel> searchList = StockStatusConnectController.ConnectStockStatus(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.StockStatusList = searchList.ToPagedList();

                    foreach (var item in model.StockStatusList)
                    {
                        // ロット番号チェック
                        string supplierProductNumberTag = $@"<td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>";
                        if (item.StoreInQuantity > 0 || item.StoreOutQuantity > 0)
                        {
                            supplierProductNumberTag = $@"<td>
                                <a href='#' onclick='OnLinkDetailClick(this)' data-toggle='modal' data-target='#detail-by-link-modal'>
                                {item.SupplierProductNumber}
                                </a>
                            </td>";
                        }

                        // 在庫数=月初在庫数+当月入庫数総計-当月出庫数総計
                        item.StockRemainQuantity = @item.StockQuantityAtBeginningMonth + (item.StoreInQuantity - item.StoreOutQuantity);
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-secondary btn-icon-split ml-1 mr-1'
                            onclick='OnDetailClick(this)' data-id='{item.ProductID}' data-toggle='modal' data-target='#detail-modal'>
                                <i class='fa-solid fa-list'></i>
                            </a>
                        </td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        {supplierProductNumberTag}
                        <td class='LotQuantity'>{@item.LotQuantity}</td>
                        <td class='StockQuantityAtBeginningMonth'>{@item.StockQuantityAtBeginningMonth}</td>
                        <td class='StoreInNumberOfBoxes'>{@item.StoreInNumberOfBoxes}</td>
                        <td class='StoreInQuantity'>{@item.StoreInQuantity}</td>
                        <td class='StoreOutNumberOfBoxes'>{@item.StoreOutNumberOfBoxes}</td>
                        <td class='StoreOutQuantity'>{@item.StoreOutQuantity}</td>
                        <td class='StockRemainQuantity'>{@item.StockRemainQuantity}</td>                        
                        <input type='hidden' class='ProductID' value='{item.ProductID}' />
                        <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                        <input type='hidden' class='DepoID' value='{item.DepoID}' />
                        <input type='hidden' class='SearchDate' value='{searchModel.DateSearchStart}' />
                        <input type='hidden' class='SupplierProductNumber' value='{item.SupplierProductNumber}' />
                        </tr>";
                    }
                }

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Content(errorMessage);
            }
        }

        /// <summary>
        /// 日別在庫照会画面表示
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        public IActionResult Detail(string searchDate, int depoId, int companyId, string supplierProductNumber)
        {
            StockStatusModel model = new();
            List<StockStatusModel> detailList = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先品番が一致する在庫情報取得(日毎の入庫数・出庫数)
                List<StockStatusModel> searchProductResult = GetSearchProductResult(searchDate, depoId, companyId, supplierProductNumber, model, user);

                // ○月1日の在庫数＝月初在庫数
                DateTime date = Convert.ToDateTime(searchDate);
                int remainQuantityPreviousDay = model.StockQuantityAtBeginningMonth;

                // ○月1日から検索日まで順に計算
                for (int i = 1; i <= date.Day; i++)
                {
                    DateTime checkDate = new(date.Year, date.Month, i);
                    StockStatusModel newItem = new()
                    {
                        WorkedDate = checkDate.ToString("yyyy/MM/dd")
                    };

                    // 仕入先品番が一致する在庫情報に日付が一致する入庫または出庫データがあるかチェック
                    var checkItem = searchProductResult.Where(item => Convert.ToDateTime(item.WorkedDate) == checkDate).FirstOrDefault();

                    // ある場合は入庫箱数・入庫数量・出庫箱数・出庫数量を格納
                    if (checkItem != null)
                    {
                        newItem.StoreInNumberOfBoxes = checkItem.StoreInNumberOfBoxes;
                        newItem.StoreInQuantity = checkItem.StoreInNumberOfBoxes * model.LotQuantity;
                        newItem.StoreOutNumberOfBoxes = checkItem.StoreOutNumberOfBoxes;
                        newItem.StoreOutQuantity = checkItem.StoreOutNumberOfBoxes * model.LotQuantity;
                    }
                    // その日の在庫数を計算
                    remainQuantityPreviousDay += (newItem.StoreInQuantity - newItem.StoreOutQuantity);
                    newItem.StockRemainQuantity = remainQuantityPreviousDay;

                    detailList.Add(newItem);
                }
                model.DetailList = detailList;

                return View(model);
            }
            catch (SqlException)
            {
                ViewData["ErrorMessage"] = "E3004: " + ErrorMessagesResources.E3004;
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 日別在庫照会画面表示
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        public IActionResult GetLotNumberDetail(string searchDate, int depoId, int companyId, string supplierProductNumber)
        {
            StockStatusModel model = new();
            List<StockStatusModel> detailList = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 仕入先品番で品番取得
                var product = M_ProductConnectController.GetProductBySupplierProductNumber(supplierProductNumber, user.DatabaseName);
                if (product == null)
                {
                    throw new Exception();
                }
                model.LotQuantity = product.LotQuantity;

                // 日別在庫照会画面表示
                List<StockStatusModel> searchProductResult = GetLotNumberDetail(searchDate, depoId, companyId, supplierProductNumber, user.DatabaseName);
                detailList = searchProductResult.Where(item => item.StockRemainQuantity != 0).ToList();

                var searchData = string.Empty;
                // 表示用のhtml作成
                if (detailList.Count > 0)
                {
                    foreach (var item in detailList)
                    {
                        searchData += $@"<tr>
                            <td class='SupplierName'>{@item.LotNumber}</td>
                            <td class='SupplierName'>{@item.StoreInNumberOfBoxes}</td>
                            <td class='SupplierName'>{@item.StoreInQuantity}</td>
                            <td class='SupplierName'>{@item.StoreOutNumberOfBoxes}</td>
                            <td class='SupplierName'>{@item.StoreOutQuantity}</td>
                            <td class='SupplierName'>{@item.StockRemainQuantity}</td>
                        </tr>";
                    }
                }

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Content(errorMessage);
            }
        }

        /// <summary>
        /// 仕入先品番が一致する在庫情報取得(日毎の入庫数・出庫数)
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <param name="model">モデル</param>
        /// <param name="user">ログインユーザー</param>
        private static List<StockStatusModel> GetSearchProductResult(string searchDate, int depoId, int companyId, string supplierProductNumber, StockStatusModel model, LoginUserModel? user)
        {
            // 在庫情報取得
            GetTotalProductResult(searchDate, depoId, companyId, supplierProductNumber, model, user);

            // 仕入先品番が一致する在庫情報取得(日毎の入庫数・出庫数)
            var productSearchSql = StockStatusConnectController.CreateSQLToGetStockStatusByProductNumber(
                searchDate, depoId, companyId, supplierProductNumber);
            List<StockStatusModel> searchProductResult = StockStatusConnectController.ConnectStockStatus(productSearchSql, user.DatabaseName);
            return searchProductResult;
        }

        /// <summary>
        /// 品番別ロット番号一覧取得
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <param name="model">モデル</param>
        /// <param name="databaseName">データベース名</param>
        private static List<StockStatusModel> GetLotNumberDetail(string searchDate, int depoId, int companyId, string supplierProductNumber, string databaseName)
        {
            // 品番別ロット番号一覧取得
            var productSearchSql = StockStatusConnectController.CreateSQLToGetLotNumberDetailByProductNumber(
                searchDate, depoId, companyId, supplierProductNumber);
            List<StockStatusModel> searchProductResult = StockStatusConnectController.ConnectStockStatus(productSearchSql, databaseName);
            return searchProductResult;
        }

        /// <summary>
        /// 在庫情報取得
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <param name="model">モデル</param>
        /// <param name="user">ログインユーザー</param>
        private static void GetTotalProductResult(string searchDate, int depoId, int companyId, string supplierProductNumber, StockStatusModel model, LoginUserModel? user)
        {
            // 倉庫IDから倉庫情報を取得
            var searchDepoSql = M_DepoConnectController.CreateSQLToSelectByDepoId(depoId);
            List<M_DepoModel> depoSearchList = M_DepoConnectController.ConnectMDepos(searchDepoSql, user.DatabaseName);

            // 会社IDから会社情報を取得
            var searchCompanySql = M_CompanyConnectController.CreateSQLToSelectByCompanyId(companyId);
            List<M_CompanyModel> companySearchList = M_CompanyConnectController.ConnectMCompanys(searchCompanySql, user.DatabaseName);

            model.DateSearchStart = searchDate;
            model.DepoID = depoId;
            model.DepoName = depoSearchList[0].DepoName;
            model.SupplierID = companyId;
            model.SupplierName = string.Concat(companySearchList[0].CompanyName, " - ", companySearchList[0].ClientName);
            model.SupplierProductNumber = supplierProductNumber;

            // 在庫情報取得
            var sql = StockStatusConnectController.CreateSQLToGetStockStatus(
                searchDate, depoId, companyId, supplierProductNumber);
            StockStatusModel searchResult = StockStatusConnectController.ConnectStockStatus(sql, user.DatabaseName).FirstOrDefault();

            // 表示用のhtml作成
            if (searchResult != null)
            {
                model.SupplierProductNumber = searchResult.SupplierProductNumber;
                model.LotQuantity = searchResult.LotQuantity;
                model.StockQuantityAtBeginningMonth = searchResult.StockQuantityAtBeginningMonth;
                model.StoreInNumberOfBoxes = searchResult.StoreInNumberOfBoxes;
                model.StoreInQuantity = searchResult.StoreInQuantity;
                model.StoreOutNumberOfBoxes = searchResult.StoreOutNumberOfBoxes;
                model.StoreOutQuantity = searchResult.StoreOutQuantity;
                // 在庫数=月初在庫数+当月入庫数総計-当月出庫数総計
                model.StockRemainQuantity = searchResult.StockQuantityAtBeginningMonth + (searchResult.StoreInQuantity - searchResult.StoreOutQuantity);
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
                DataTable searchResult = CreateDataTable();

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 在庫照会情報取得
                var sql = StockStatusConnectController.CreateSQLToGetStockStatus(
                    searchModel.SearchStartDate, searchModel.DepoID, searchModel.CompanyID);
                List<StockStatusModel> searchList = StockStatusConnectController.ConnectStockStatus(sql, user.DatabaseName);

                // DataRowに格納
                if (searchList.Count > 0)
                {
                    foreach (StockStatusModel item in searchList)
                    {
                        var stockRemainQuantity = item.StockQuantityAtBeginningMonth + (item.StoreInQuantity - item.StoreOutQuantity);
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<StockStatusModel>("SupplierName")] = item.SupplierName.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StockQuantityAtBeginningMonth")] = item.StockQuantityAtBeginningMonth.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreInNumberOfBoxes")] = item.StoreInNumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreInQuantity")] = item.StoreInQuantity.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreOutNumberOfBoxes")] = item.StoreOutNumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreOutQuantity")] = item.StoreOutQuantity.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StockRemainQuantity")] = stockRemainQuantity.ToString();

                        searchResult.Rows.Add(newRow);
                    }
                }

                // ファイル名作成
                string fileName = CreateFile.CreateFileName(searchModel, gamenName);

                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), fileName);

                // DataTableをCSV形式の文字列に変換
                CreateFile.ConvertDataTableToCsv(searchResult, filePath);

                // ファイルの作成
                var fileResult = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(fileResult, System.Net.Mime.MediaTypeNames.Application.Octet, fileName) });
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
        /// ファイル出力(日別在庫照会)
        /// </summary>
        /// <param name="searchModel">詳細モデル</param>
        /// <param name="gamenName">画面名</param>
        [HttpPost]
        [Route("StockStatus/Detail/ExportCsv")]
        public JsonResult ExportCsvDetail(SearchConditionModel searchModel, string gamenName)
        {
            try
            {
                // DataTable作成
                DataTable searchResult = CreateDataTableForDetail();

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                StockStatusModel model = new();
                List<StockStatusModel> detailList = new();

                // 仕入先品番が一致する在庫情報取得(日毎の入庫数・出庫数)
                List<StockStatusModel> searchProductResult = GetSearchProductResult(
                    searchModel.SearchStartDate, searchModel.DepoID, searchModel.CompanyID, searchModel.SupplierProductNumber, model, user);

                // ○月1日の在庫数＝月初在庫数
                DateTime date = Convert.ToDateTime(searchModel.SearchStartDate);
                int remainQuantityPreviousDay = model.StockQuantityAtBeginningMonth;

                // ○月1日から検索日まで順に計算
                for (int i = 1; i <= date.Day; i++)
                {
                    DateTime checkDate = new(date.Year, date.Month, i);
                    StockStatusModel newItem = new()
                    {
                        WorkedDate = checkDate.ToString("yyyy/MM/dd")
                    };

                    // 仕入先品番が一致する在庫情報に日付が一致する入庫または出庫データがあるかチェック
                    var checkItem = searchProductResult.Where(item => Convert.ToDateTime(item.WorkedDate) == checkDate).FirstOrDefault();

                    // ある場合は入庫箱数・入庫数量・出庫箱数・出庫数量を格納
                    if (checkItem != null)
                    {
                        newItem.StoreInNumberOfBoxes = checkItem.StoreInNumberOfBoxes;
                        newItem.StoreInQuantity = checkItem.StoreInNumberOfBoxes * model.LotQuantity;
                        newItem.StoreOutNumberOfBoxes = checkItem.StoreOutNumberOfBoxes;
                        newItem.StoreOutQuantity = checkItem.StoreOutNumberOfBoxes * model.LotQuantity;
                    }
                    // その日の在庫数を計算
                    remainQuantityPreviousDay += (newItem.StoreInQuantity - newItem.StoreOutQuantity);
                    newItem.StockRemainQuantity = remainQuantityPreviousDay;

                    detailList.Add(newItem);
                }
                model.DetailList = detailList;

                // DataRowに格納
                if (detailList.Count > 0)
                {
                    foreach (StockStatusModel item in detailList)
                    {
                        var stockRemainQuantity = item.StockQuantityAtBeginningMonth + (item.StoreInQuantity - item.StoreOutQuantity);
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<StockStatusModel>("WorkedDate")] = item.WorkedDate;
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreInNumberOfBoxes")] = item.StoreInNumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreInQuantity")] = item.StoreInQuantity.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreOutNumberOfBoxes")] = item.StoreOutNumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StoreOutQuantity")] = item.StoreOutQuantity.ToString();
                        newRow[Utils.GetDisplayName<StockStatusModel>("StockRemainQuantity")] = item.StockRemainQuantity.ToString();

                        searchResult.Rows.Add(newRow);
                    }
                }

                // ファイル名作成
                string fileName = CreateFile.CreateFileName(searchModel, gamenName);

                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), fileName);

                // DataTableをCSV形式の文字列に変換
                CreateFile.ConvertDataTableToCsv(searchResult, filePath);

                // ファイルの作成
                var fileResult = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(fileResult, System.Net.Mime.MediaTypeNames.Application.Octet, fileName) });
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

            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("LotQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StockQuantityAtBeginningMonth"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreInNumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreInQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreOutNumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreOutQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StockRemainQuantity"), typeof(string));

            return table;
        }

        /// <summary>
        /// データテーブル作成(日別在庫照会用)
        /// </summary>
        /// <returns></returns>
        private static DataTable CreateDataTableForDetail()
        {
            var table = new DataTable();

            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("WorkedDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreInNumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreInQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreOutNumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StoreOutQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<StockStatusModel>("StockRemainQuantity"), typeof(string));

            return table;
        }
    }
}
