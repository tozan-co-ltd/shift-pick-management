using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
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

                // 在庫情報取得SQL作成
                var sql = StockStatusConnectController.CreateSQLToGetStockStatus(
                    searchModel.DateSearchStart, searchModel.SelectedDepoID, searchModel.SelectedCompanyID);
                // DB接続
                List<StockStatusModel> searchList = StockStatusConnectController.ConnectStockStatus(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.StockStatusList = searchList.ToPagedList();

                    foreach (var item in model.StockStatusList)
                    {
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
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
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
        /// 仕入先品番で在庫照会情報取得
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        public IActionResult Detail(string searchDate, int depoId, int companyId, string supplierProductNumber)
        {
            StockStatusModel model = new();
            //var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                model.DateSearchStart = searchDate;
                // 倉庫IDで倉庫情報を取得
                var searchDepoSql = M_DepoConnectController.CreateSQLToSelectByDepoId(depoId);
                List<M_DepoModel> depoSearchList = M_DepoConnectController.ConnectMDepos(searchDepoSql, user.DatabaseName);
                if (depoSearchList.Count != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }
                model.DepoID = depoSearchList[0].DepoID;
                model.DepoName = depoSearchList[0].DepoName;

                // 会社IDで会社情報を取得
                var searchCompanySql = M_CompanyConnectController.CreateSQLToSelectByCompanyId(companyId);
                List<M_CompanyModel> companySearchList = M_CompanyConnectController.ConnectMCompanys(searchCompanySql, user.DatabaseName);
                if (companySearchList.Count != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }
                model.SupplierID = companySearchList[0].CompanyID;
                model.SupplierName = companySearchList[0].CompanyName;

                // 在庫情報取得SQL作成
                var sql = StockStatusConnectController.CreateSQLToGetStockStatus(
                    searchDate, depoId, companyId, supplierProductNumber);
                // DB接続
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

                // 仕入先品番で在庫情報取得SQL作成
                var productSearchSql = StockStatusConnectController.CreateSQLToGetStockStatusByProductNumber(
                    searchDate, depoId, companyId, supplierProductNumber);
                // DB接続
                List<StockStatusModel> searchProductResult = StockStatusConnectController.ConnectStockStatus(productSearchSql, user.DatabaseName);
                List<StockStatusModel> detailList = new();
                DateTime date = Convert.ToDateTime(searchDate);
                int remainQuantity = model.StockQuantityAtBeginningMonth;
                for (int i = 1; i <= date.Day; i++)
                {
                    DateTime checkDate = new DateTime(date.Year, date.Month, i);
                    StockStatusModel newItem = new();

                    newItem.WorkedDate = checkDate.ToString("yyyy/MM/dd");
                    var checkItem = searchProductResult.Where(item => Convert.ToDateTime(item.WorkedDate) == checkDate).FirstOrDefault();
                    if (checkItem != null)
                    {
                        newItem.StoreInNumberOfBoxes = checkItem.StoreInNumberOfBoxes;
                        newItem.StoreInQuantity = checkItem.StoreInNumberOfBoxes * model.LotQuantity;
                        newItem.StoreOutNumberOfBoxes = checkItem.StoreOutNumberOfBoxes;
                        newItem.StoreOutQuantity = checkItem.StoreOutNumberOfBoxes * model.LotQuantity;
                    }
                    remainQuantity = remainQuantity + (newItem.StoreInQuantity - newItem.StoreOutQuantity);
                    newItem.StockRemainQuantity = remainQuantity;

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
    }
}
