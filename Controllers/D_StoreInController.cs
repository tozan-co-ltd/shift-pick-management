using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 入庫実績照会・修正画面
    /// </summary>
    public class D_StoreInController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // 新規作成行数
        private const int InitRegisterRowCount = 5;

        /// <summary>
        /// 入庫実績照会画面表示
        /// </summary>
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
                model.AuthorizedKubun = user.AuthorizedKubun;

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
                ModelState.Remove("Quantity");
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 入庫実績情報取得
                var sql = D_StoreInConnectController.CreateSQLToSelectDStoreIns(searchModel);
                List<D_StoreInModel> storeInList = D_StoreInConnectController.ConnectDStoreIns(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (storeInList.Count > 0)
                {
                    model.DStoreInList = storeInList.ToPagedList();

                    foreach (var item in model.DStoreInList)
                    {
                        // 管理権限区分が1(管理者)のみ削除ボタン表示
                        if (user.AuthorizedKubun == 1)
                        {
                            searchData += $@"
                                <tr>
                                    <td>
                                        <a class='btn btn-success btn-icon-split ml-1 mr-1 btn-success-store-in'
                                        onclick='OnEditClick(this)' data-id='{item.StoreInID}' data-toggle='modal' data-target='#edit-modal'>
                                            <i class='fa-solid fa-pen'></i>
                                        </a>
                                        <button class='btn btn-danger btn-icon-split btn-danger-store-in'
                                        onclick='OnDeleteClick(this)' data-id='{item.StoreInID}' data-toggle='modal' data-target='#delete-modal'>
                                            <i class='fa-solid fa-trash'></i>
                                        </button>
                                </td>
                            ";
                        }
                        else
                        {
                            searchData += $@"<tr><td></td>";
                        }

                        searchData += $@"
                            <td class='StoreInID'>{@item.StoreInID}</td>
                            <td class='SupplierName'>{@item.SupplierName}</td>
                            <td class='StoreInDate'>{Utils.ConvertToYYYYMMDD(@item.StoreInDate)}</td>
                            <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                            <td class='LotNumber'>{@item.LotNumber}</td>
                            <td class='LotQuantity'>{Utils.FormatNumber(@item.LotQuantity)}</td>
                            <td class='NumberOfBoxes'>{Utils.FormatNumber(@item.NumberOfBoxes)}</td>
                            <td class='Quantity'>{Utils.FormatNumber(@item.Quantity)}</td>
                            <td class='MainProductKey'>{@item.MainProductKey}</td>
                            <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                            <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                            <td class='Remarks'>{@item.Remarks}</td>
                            <td class='UpdatedAt'>{@item.UpdatedAt:yyyy/MM/dd HH:mm}</td>
                            <td class='UpdatedBy'>{@item.UpdatedBy}</td>
                            <input type='hidden' class='DepoID' value='{item.DepoID}' />
                            <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                            </tr>
                        ";
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
            D_StoreInModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // メイン倉庫を初期選択
                List<D_StoreInModel> storeInList = new();
                for (int i = 0; i < InitRegisterRowCount; i++)
                {
                    D_StoreInModel viewModel = new()
                    {
                        DepoID = user.MainDepoID
                    };
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
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                List<string> errorMessageList = new();

                int registerCount = 0;
                for (int i = 0; i < model.RegisterList.Count; i++)
                {
                    var modelItem = model.RegisterList[i];
                    if (modelItem.SupplierProductNumber.Equals("0")) continue;
                    registerCount++;

                    var validationContext = new ValidationContext(modelItem);
                    var validationResults = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(modelItem, validationContext, validationResults, true);
                    List<string> errorMembers = validationResults.SelectMany(result => result.MemberNames).Distinct().ToList();

                    // 仕入先品番チェック
                    bool isContainSupplierProductNumber = errorMembers.Contains("SupplierProductNumber");
                    if (!isContainSupplierProductNumber)
                    {
                        // 倉庫ID,仕入先ID,仕入先品番が一致するレコードが品番マスターあるかチェック
                        var product = M_ProductConnectController.GetProductBySupplierProductNumber(
                            model.SelectedDepoID, model.SelectedCompanyID, modelItem.SupplierProductNumber, user.DatabaseName);
                        if (product == null)
                        {
                            isValid = false;
                            var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreInModel>("SupplierProductNumber"));
                            validationResults.Add(new ValidationResult(message, new List<string> { "SupplierProductNumber" }));
                        }
                        else
                        {
                            model.RegisterList[i].LotQuantity = product.LotQuantity;
                            model.RegisterList[i].SupplierProductNumber = product.SupplierProductNumber;
                        }
                    }

                    // エラーメッセージ作成
                    if (!isValid)
                    {
                        var lineCount = i + 1;
                        foreach (var err in validationResults)
                        {
                            // フォーマットエラーメッセージ
                            List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ReceiveScheduleModel>(err);
                            errorMessageItem.Insert(0, lineCount + "行目");

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

                // リストチェック
                if (registerCount == 0)
                {
                    throw new Exception();
                }

                // エラーが1件以上ある場合はreturn
                if (errorMessageList.Count > 0)
                {
                    errorMessage = string.Join("</br>", errorMessageList);

                    // log取得
                    _logger.Error($"入庫実績登録失敗");

                    return NotFound(new { errorMessage });
                }

                model.RegisterList = model.RegisterList.Where(x => !x.SupplierProductNumber.Equals("0")).ToList();
                // 入庫実績登録
                D_StoreInConnectController.InsertDStoreIns(model, user);

                // log取得
                _logger.Info($"入庫実績登録成功 入庫日:{model.SearchStartDate}");

                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
        }

        /// <summary>
        /// 入庫実績更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(D_StoreInModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1017: " + ErrorMessagesResources.E1017;
                    _logger.Error($"入庫実績更新失敗 {errorMessage}");
                    return NotFound(new { errorMessage });
                }

                // 仕入先品番チェック
                var product = M_ProductConnectController.GetProductBySupplierProductNumber(
                    model.SelectedDepoID, model.SelectedCompanyID, model.SupplierProductNumber, user.DatabaseName);
                if (product == null)
                {
                    // log取得
                    errorMessage = errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"));
                    _logger.Error($"入庫実績更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }
                model.DepoID = model.SelectedDepoID;
                model.CompanyID = model.SelectedCompanyID;
                model.NumberOfBoxes = (int)Math.Ceiling((double)model.Quantity / product.LotQuantity);

                // 入庫実績更新
                D_StoreInConnectController.UpdateDStoreIn(model, user);

                // log取得
                _logger.Info($"入庫実績更新成功 入庫実績ID:{model.StoreInID}");

                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
        }

        /// <summary>
        /// 入庫実績削除
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <returns></returns>
        public IActionResult Delete(int storeInId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入庫実績削除
                D_StoreInConnectController.DeleteDStoreIn(storeInId, user);

                // log取得
                _logger.Info($"入庫実績削除成功 入庫実績ID:{storeInId}");

                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
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

                // 入庫実績情報取得
                D_StoreInModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                };
                var sql = D_StoreInConnectController.CreateSQLToSelectDStoreIns(model);
                List<D_StoreInModel> searchList = D_StoreInConnectController.ConnectDStoreIns(sql, user.DatabaseName);

                // DataRowに格納
                if (searchList.Count > 0)
                {
                    foreach (D_StoreInModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("StoreInID")] = item.StoreInID.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SupplierName")] = item.SupplierName;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("StoreInDate")] = Utils.ConvertToYYYYMMDD(item.StoreInDate);
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SupplierProductNumber")] = item.SupplierProductNumber;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("LotNumber")] = item.LotNumber;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_StoreInModel>("MainProductKey")] = item.MainProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("FirstSubProductKey")] = item.FirstSubProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("SecondSubProductKey")] = item.SecondSubProductKey;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("Remarks")] = item.Remarks;
                        newRow[Utils.GetDisplayName<D_StoreInModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<D_StoreInModel>("UpdatedBy")] = item.UpdatedBy;

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
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreInModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
