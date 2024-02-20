using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    public class D_StoreInController : BaseController
    {
        /// <summary>
        /// 入庫 - 入荷実績照会
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(D_StoreInModel model)
        {
            try
            {
                if (model == null)
                    model = new D_StoreInModel();

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 入庫実績情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_StoreInModel searchModel)
        {
            D_StoreInModel model = new D_StoreInModel();
            var searchData = string.Empty;
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

                // 検索情報をチェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "正しい入力値を入力してください。" });
                }

                // 入庫実績情報取得SQL作成
                var sql = D_StoreInConnectionController.CreateSQLToGetDStoreIns(
                    searchModel.DateSearchStart, searchModel.DateSearchEnd, searchModel.SelectedDepoID, searchModel.SelectedCompanyID);
                // DB接続
                List<D_StoreInModel> storeInList = D_StoreInConnectionController.ConnectDStoreIns(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (storeInList.Count > 0)
                {
                    model.D_StoreInList = storeInList.ToPagedList();

                    foreach (var item in model.D_StoreInList)
                    {
                        searchData = $@"<tr>
                        <td>
                            <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                            onclick=""OnEditClick('{item.StoreInID}')"" data-id=""{item.StoreInID}"" data-toggle=""modal"" data-target=""#edit-modal"">
                                <i class=""fa-solid fa-pen""></i>
                            </a>
                            <button class=""btn btn-danger btn-icon-split""
                            onclick=""OnDeleteClick('{item.StoreInID}')"" data-id=""{item.StoreInID}"" data-toggle=""modal"" data-target=""#delete-modal"">
                                <i class=""fa-solid fa-trash""></i>
                            </button>
                        </td>
                        <td>{@item.StoreInID}</td>
                        <td>{@item.SupplierName}</td>
                        <td>{@item.StoreInDate}</td>
                        <td>{@item.SupplierProductNumber}</td>
                        <td>{@item.LotNumber}</td>
                        <td>{@item.LotQuantity}</td>
                        <td>{@item.Quantity}</td>
                        <td>{@item.NumberOfBoxes}</td>
                        <td>{@item.MainProductKey}</td>
                        <td>{@item.FirstSubProductKey}</td>
                        <td>{@item.SecondSubProductKey}</td>
                        <td>{@item.Remarks}</td>
                        <td>{@item.CreatedAt.ToString("yyyy/MM/ddHH:mm:ss")}</td>
                        <td>{@item.CreatedBy}</td>
                        </tr>";
                    }
                }

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = ErrorMessagesResources.E4001 });
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                return Content(exceptionMessage);
            }
        }
    }
}
