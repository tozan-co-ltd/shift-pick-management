using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 入荷実績照会画面
    /// </summary>
    public class D_ReceiveController : BaseController
    {
        /// <summary>
        /// 入荷実績照会画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_ReceiveModel model = new();
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
        /// 入荷実績情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_ReceiveModel searchModel)
        {
            D_ReceiveModel model = new();
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 検索情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "正しい入力値を入力してください。" });
                }

                // 入庫実績情報取得SQL作成
                var sql = D_ReceiveConnectController.CreateSQLToGetDReceives(
                    searchModel.SearchStartDate, searchModel.SearchEndDate, searchModel.SelectedDepoID, searchModel.SelectedCompanyID);
                // DB接続
                List<D_ReceiveModel> searchList = D_ReceiveConnectController.ConnectDReceives(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.D_ReceiveModelList = searchList.ToPagedList();

                    foreach (var item in model.D_ReceiveModelList)
                    {
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-success btn-icon-split ml-1 mr-1'
                            onclick='OnEditClick(this)' data-id='{item.ReceiveID}' data-toggle='modal' data-target='#edit-modal'>
                                <i class='fa-solid fa-pen'></i>
                            </a>
                            <button class='btn btn-danger btn-icon-split'
                            onclick='OnDeleteClick(this)' data-id='{item.ReceiveID}' data-toggle='modal' data-target='#delete-modal'>
                                <i class='fa-solid fa-trash'></i>
                            </button>
                        </td>
                        <td class='ReceiveID'>{@item.ReceiveID}</td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='ReceiveDatetime'>{@item.ReceiveDatetime}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotNumber'>{@item.LotNumber}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='MainProductKey'>{@item.MainProductKey}</td>
                        <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                        <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                        <td class='CreatedAt'>{@item.CreatedAt.ToString("yyyy/MM/dd")}</td>
                        <td class='ScanedAt'>{@item.ScanedAt.ToString("yyyy/MM/dd")}</td>
                        <td class='CreatedBy'>{@item.CreatedBy}</td>
                        <input type='hidden' class='DepoID' value='{item.DepoID}' />
                        <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                        <input type='hidden' class='ScanResultID' value='{item.ScanResultID}'/>
    
                        <input type='hidden' class='HandyMenuID' value='{item.HandyMenuID}'/>
                        <input type='hidden' class='SupplierKanbanID' value='{item.SupplierKanbanID}'/>
                        <input type='hidden' class='NumberOfInputBoxes' value='{item.NumberOfInputBoxes}'/>
                        <input type='hidden' class='FirstScanedString' value='{item.FirstScanedString}'/>
                        <input type='hidden' class='SecondScanedString' value='{item.SecondScanedString}'/>
                        <input type='hidden' class='NumberOfInputBoxes' value='{item.NumberOfInputBoxes}'/>
                        <input type='hidden' class='FirstScanedString' value='{item.FirstScanedString}'/>
                        <input type='hidden' class='SecondScanedString' value='{item.SecondScanedString}'/>
                        </tr>";
                    }
                }

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                return Content(exceptionMessage);
            }
        }
    }
}
