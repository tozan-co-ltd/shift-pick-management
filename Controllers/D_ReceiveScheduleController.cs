using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 入荷予定照会画面
    /// </summary>
    public class D_ReceiveScheduleController : BaseController
    {
        /// <summary>
        /// 入荷予定照会画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_ReceiveScheduleModel model = new();
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
        /// 入荷予定情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_ReceiveScheduleModel searchModel)
        {
            D_ReceiveScheduleModel model = new();
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                ModelState.Remove("CompanyCode");
                ModelState.Remove("Quantity");
                ModelState.Remove("LotNumber");
                ModelState.Remove("SupplierProductNumber");
                ModelState.Remove("ReceiveScheduleDate");
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 入庫実績情報取得SQL作成
                var sql = D_ReceiveScheduleConnectController.CreateSQLToSelectDReceiveSchedules(searchModel);
                // DB接続
                List<D_ReceiveScheduleModel> searchList = D_ReceiveScheduleConnectController.ConnectDReceiveSchedules(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.D_ReceiveScheduleList = searchList.ToPagedList();

                    foreach (var item in model.D_ReceiveScheduleList)
                    {
                        searchData += $@"<tr>
                        <td class='ReceiveScheduleID'>{@item.ReceiveScheduleID}</td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='ReceiveScheduleDate'>{Utils.ConvertToYYYYMMDD(@item.ReceiveScheduleDate)}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotNumber'>{@item.LotNumber}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='StoreInNumberOfBox'>{@item.StoreInNumberOfBox}</td>
                        <td class='StoreInQuantity'>{@item.StoreInQuantity}</td>
                        <td class='CreatedAt'>{@item.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss")}</td>
                        <td class='CreatedBy'>{@item.CreatedBy}</td>
                        <input type='hidden' class='DepoID' value='{item.DepoID}' />
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
