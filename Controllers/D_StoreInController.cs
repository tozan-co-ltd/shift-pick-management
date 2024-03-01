using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 入庫実績照会・修正画面
    /// </summary>
    public class D_StoreInController : BaseController
    {
        // 新規作成行数
        private const int InitRegisterRowCount = 5;

        /// <summary>
        /// 入庫実績照会画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_StoreInModel model = new();
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
        /// 入庫実績情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_StoreInModel searchModel)
        {
            D_StoreInModel model = new();
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                ModelState.Remove("SupplierProductNumber");
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 入庫実績情報取得SQL作成
                var sql = D_StoreInConnectionController.CreateSQLToSelectDStoreIns(searchModel);
                // DB接続
                List<D_StoreInModel> storeInList = D_StoreInConnectionController.ConnectDStoreIns(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (storeInList.Count > 0)
                {
                    model.DStoreInList = storeInList.ToPagedList();

                    foreach (var item in model.DStoreInList)
                    {
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-success btn-icon-split ml-1 mr-1'
                            onclick='OnEditClick(this)' data-id='{item.StoreInID}' data-toggle='modal' data-target='#edit-modal'>
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

        /// <summary>
        /// 登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null)
                {
                    return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
                }

                var model = new D_StoreInModel();
                List<D_StoreInModel> storeInList = new();
                for (int i = 0; i < InitRegisterRowCount; i++)
                {
                    var viewModel = new D_StoreInModel();
                    viewModel.DepoID = user.MainDepoID;
                    storeInList.Add(viewModel);

                    model.RegisterList = storeInList;
                }
                // 会社リスト取得
                CommonModel commonModel = new();
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_SupplierID);

                return View(model);
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999 :" + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 入庫実績登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(D_StoreInModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                List<string> errorMessageList = new();
                int readCount = 1;
                // リストチェック
                if (model.RegisterList != null && model.RegisterList.Count > 0)
                {
                    foreach(var modelItem in model.RegisterList)
                    {
                        var validationContext = new ValidationContext(modelItem);
                        var validationResults = new List<ValidationResult>();
                        bool isValid = Validator.TryValidateObject(modelItem, validationContext, validationResults, true);
                        List<string> errorMembers = validationResults.SelectMany(result => result.MemberNames).Distinct().ToList();

                        // 仕入先品番チェック
                        bool isContainSupplierProductNumber = errorMembers.Contains("SupplierProductNumber");
                        if (!isContainSupplierProductNumber)
                        {
                            // 仕入先品番で品番チェック
                            bool isExistProduct = M_ProductConnectController.IsExistedSupplierProductNumber(modelItem.SupplierProductNumber, user.DatabaseName);
                            if (!isExistProduct)
                            {
                                isValid = false;
                                var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreInModel>("SupplierProductNumber"));
                                validationResults.Add(new ValidationResult(message, new List<string> { "SupplierProductNumber" }));
                            }
                        }

                        // エラーメッセージ作成
                        if (!isValid)
                        {
                            foreach (var err in validationResults)
                            {
                                // フォーマットエラーメッセージ
                                List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ReceiveScheduleModel>(err);
                                errorMessageItem.Insert(0, readCount + "行目");

                                // HTMLに変換
                                var errorHtml = string.Empty;
                                foreach (var item in errorMessageItem)
                                {
                                    errorHtml += "<td class='pl-2 pr-2'>" + item.ToString() + "</td>";
                                }
                                errorHtml = "<tr>" + errorHtml + "</tr>";

                                errorMessageList.Add(errorHtml);
                            }
                        }
                    }

                    // エラーが1件以上ある場合はreturn
                    if (errorMessageList.Count > 0)
                    {
                        var errorMessage = string.Join("</br>", errorMessageList);
                        return NotFound(new { errorMessage });
                    }
                }

                // 入庫実績登録
                D_StoreInConnectionController.InsertDStoreIns(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004 :" + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999 :" + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// 入庫実績更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(D_StoreInModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 仕入先品番チェック
                bool isExistProduct = M_ProductConnectController.IsExistedSupplierProductNumber(model.SupplierProductNumber, user.DatabaseName);
                if (!isExistProduct)
                {
                    var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"));
                    return NotFound(new { errorMessage = message });
                }

                // 入庫実績更新
                D_StoreInConnectionController.EditDStoreIn(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004 :" + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999 :" + ErrorMessagesResources.E9999 });
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

                // 入庫実績情報取得
                D_StoreInModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                };
                var sql = D_StoreInConnectionController.CreateSQLToSelectDStoreIns(model);

                //var sql = D_StoreInConnectionController.CreateSQLToSelectDStoreIns(
                //    searchModel.SearchStartDate, searchModel.SearchEndDate, searchModel.DepoID, searchModel.CompanyID);
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
        /// データテーブル作成
        /// </summary>
        /// <returns></returns>
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
