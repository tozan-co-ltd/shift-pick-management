using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出荷指示照会画面
    /// </summary>
    public class D_ShipmentScheduleController : BaseController
    {
        /// <summary>
        /// 出荷指示照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_ShipmentScheduleSearchModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社リスト取得
                CommonModel commonModel = new();
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_DeliveryID);
                model.SearchDepoList = commonModel.GetMDepoList(user.DatabaseName);
                model.BinList = Utils.Const_BinList;
                model.BinList[0].Selected= true;

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 出荷指示情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_ShipmentScheduleSearchModel searchModel)
        {
            D_ShipmentScheduleModel model = new();
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

                // 出荷指示報取得SQL作成
                var sql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedules(searchModel);
                // DB接続
                List<D_ShipmentScheduleModel> searchList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.D_ShipmentScheduleList = searchList.ToPagedList();

                    foreach (var item in model.D_ShipmentScheduleList)
                    {
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-secondary btn-icon-split ml-1 mr-1'
                            onclick='OnDetailClick(this)' data-id='{item.ShipmentScheduleID}' data-toggle='modal' data-target='#detail-modal'>
                                <i class='fa-solid fa-list'></i>
                            </a>
                            <button class='btn btn-danger btn-icon-split'
                            onclick='OnDeleteClick(this)' data-id='{item.ShipmentScheduleID}' data-toggle='modal' data-target='#delete-modal'>
                                <i class='fa-solid fa-trash'></i>
                            </button>
                        </td>
                        <td class='ShipmentScheduleID'>{@item.ShipmentScheduleID}</td>
                        <td class='DeliveryName'>{@item.DeliveryName}</td>
                        <td class='DeliveryDate'>{@item.DeliveryDate}</td>
                        <td class='DeliveryTimeClass'>{@item.DeliveryTimeClass}</td>
                        <td class='DeliveryProductNumber'>{@item.DeliveryProductNumber}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotQuantity'>{@item.LotQuantity}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='StoreOutNumberOfBoxes'>{@item.StoreOutNumberOfBoxes}</td>
                        <td class='StoreOutQuantity'>{@item.StoreOutQuantity}</td>
                        <td class='OrdererCode'>{@item.OrdererCode}</td>
                        <td class='OrdererFactoryKubun'>{@item.OrdererFactoryKubun}</td>
                        <td class='OrdererName'>{@item.OrdererName}</td>
                        <td class='OrdererFactoryName'>{@item.OrdererFactoryName}</td>
                        <td class='ShipperCode'>{@item.ShipperCode}</td>
                        <td class='ShipperFactoryKubun'>{@item.ShipperFactoryKubun}</td>
                        <td class='ShipperName'>{@item.ShipperName}</td>
                        <td class='DeliveryCode'>{@item.DeliveryCode}</td>
                        <td class='DeliveryFactoryKubun'>{@item.DeliveryFactoryKubun}</td>
                        <td class='DeliveryLocation'>{@item.DeliveryLocation}</td>
                        <td class='DeliveryName'>{@item.DeliveryName}</td>
                        <td class='DeliveryFactoryName'>{@item.DeliveryFactoryName}</td>
                        <td class='RegularKubun'>{@item.RegularKubun}</td>
                        <td class='IssuedDate'>{@item.IssuedDate}</td>
                        <td class='DeliveryTime'>{@item.DeliveryTime}</td>
                        <td class='TranspotationIdentify'>{@item.TranspotationIdentify}</td>
                        <td class='DeliverySlipNumber'>{@item.DeliverySlipNumber}</td>
                        <td class='DeliverySlipPageNumber'>{@item.DeliverySlipPageNumber}</td>
                        <td class='DeliverySlipRowNumber'>{@item.DeliverySlipRowNumber}</td>
                        <td class='DeliveryProductAbbreviation'>{@item.DeliveryProductAbbreviation}</td>
                        <td class='DeliveryProductName'>{@item.DeliveryProductName}</td>
                        <td class='BranchNumber'>{@item.BranchNumber}</td>
                        <td class='UpdatedAt'>{@item.UpdatedAt}</td>
                        <td class='UpdatedBy'>{@item.UpdatedBy}</td>
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
