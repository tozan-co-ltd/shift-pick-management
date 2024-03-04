using mar_sumaken_web.Commons;
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
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
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
    }
}
