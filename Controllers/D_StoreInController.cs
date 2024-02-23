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
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-success btn-icon-split ml-1 mr-1'
                            onclick='OnEditClick({item.StoreInID})' data-id='{item.StoreInID}' data-toggle='modal' data-target='#edit-modal'>
                                <i class='fa-solid fa-pen'></i>
                            </a>
                            <button class='btn btn-danger btn-icon-split'
                            onclick='OnDeleteClick(this)' data-id='{item.StoreInID}' data-toggle='modal' data-target='#delete-modal'>
                                <i class='fa-solid fa-trash'></i>
                            </button>
                        </td>

                        <td class='StoreInID'>{@item.StoreInID}</td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='StoreInDate'>{@item.StoreInDate.ToString("yyyy/MM/dd")}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotNumber'>{@item.LotNumber}</td>
                        <td class='LotQuantity'>{@item.LotQuantity}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='MainProductKey'>{@item.MainProductKey}</td>
                        <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                        <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                        <td class='Remarks'>{@item.Remarks}</td>
                        <td class='CreatedAt'>{@item.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss")}</td>
                        <td class='CreatedBy'>{@item.CreatedBy}</td>
                        </tr>";
                    }
                }

                var response = new
                {
                    HtmlContent = searchData,
                    DataList = model.D_StoreInList
                };

                return Json(response);
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

        /// <summary>
        /// 入庫実績削除
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <returns></returns>
        public IActionResult Delete(int storeInId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || storeInId == 0)
                {
                    return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
                }

                // 入庫実績削除
                D_StoreInConnectionController.DeleteDStoreIn(storeInId, user.DatabaseName);

                return Ok();
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

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    return Json(new { res = "NG", error = ErrorMessagesResources.E9999 });
                }

                // 検索情報取得SQL作成
                var sql = D_StoreInConnectionController.CreateSQLToGetDStoreIns(
                    searchModel.SearchStartDate, searchModel.SearchEndDate, searchModel.DepoID, searchModel.CompanyID);
                // 検索情報取得
                List<D_StoreInModel> searchList = D_StoreInConnectionController.ConnectDStoreIns(sql, user.DatabaseName);
                if (searchList.Count > 0)
                {
                    foreach (D_StoreInModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("StoreInID")] = item.StoreInID.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SupplierName")] = item.SupplierName;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("StoreInDate")] = item.StoreInDate.ToString("yyyy/MM/dd");
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SupplierProductNumber")] = item.SupplierProductNumber;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("LotNumber")] = item.LotNumber;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("MainProductKey")] = item.MainProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("FirstSubProductKey")] = item.FirstSubProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SecondSubProductKey")] = item.SecondSubProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("Remarks")] = item.Remarks;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("CreatedAt")] = item.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss");
                        newRow[Utils.GetDisplayName<D_StoreInModel>("CreatedBy")] = item.CreatedBy;

                        searchResult.Rows.Add(newRow);
                    }
                }

                // ファイル名作成
                string fileName = CreateFileController.CreateFileName(searchModel, gamenName);

                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), fileName);

                // DataTableをCSV形式の文字列に変換
                CreateFileController.ConvertDataTableToCsv(searchResult, filePath);

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
                return Json(new { errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// テーブルを作る
        /// </summary>
        private static DataTable CreateDataTable()
        {
            var table = new DataTable();

            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("StoreInID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("StoreInDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("LotQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("MainProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("FirstSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("SecondSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("Remarks"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("CreatedBy"), typeof(string));

            return table;
        }
    }
}
