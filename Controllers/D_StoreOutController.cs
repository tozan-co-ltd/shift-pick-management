using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出庫実績照会・修正画面
    /// </summary>
    public class D_StoreOutController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // 新規作成行数
        private const int InitRegisterRowCount = 5;

        /// <summary>
        /// 出庫実績照会・修正画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_StoreOutModel model = new();
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
        /// 検索ボタン押下
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public IActionResult SearchData(D_StoreOutModel searchModel)
        {
            D_StoreOutModel model = new();
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                ModelState.Remove("DeliveryProductNumber");
                ModelState.Remove("SearchDeliveryDate");
                ModelState.Remove("Quantity");
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // 出庫実績情報取得
                var sql = D_StoreOutConnectController.CreateSQLToSelectDStoreOuts(searchModel);
                List<D_StoreOutModel> dStoreOutList = D_StoreOutConnectController.ConnectDStoreOuts(sql, ClaimsLoginUserData().DatabaseName);

                // 表示用のhtml作成
                if (dStoreOutList.Count > 0)
                {
                    IEnumerable<D_StoreOutModel> query = dStoreOutList.Select(s => s);
                    model.D_StoreOutList = query.ToPagedList();

                    foreach (var item in model.D_StoreOutList)
                    {
                        // 管理権限区分が1(管理者)のみ削除ボタン表示
                        if (user.AuthorizedKubun == 1)
                        {
                            searchData += $@"
                                <tr>
                                    <td>
                                        <a class='btn btn-success btn-icon-split ml-1 mr-1 btn-success-store-out'
                                        onclick='OnEditClick(this)' data-id='{item.StoreOutID}' data-toggle='modal' data-target='#edit-modal'>
                                            <i class='fa-solid fa-pen'></i>
                                        </a>
                                        <button class='btn btn-danger btn-icon-split btn-danger-store-out'
                                        onclick='OnDeleteClick(this)' data-id='{item.StoreOutID}' data-toggle='modal' data-target='#delete-modal'>
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
                            <td class='StoreOutID'>{@item.StoreOutID}</td>
                            <td class='SupplierName'>{@item.SupplierName}</td>
                            <td class='StoreOutDate'>{Utils.ConvertToYYYYMMDD(@item.StoreOutDate)}</td>
                            <td class='DeliveryDate'>{@item.DeliveryDate:yyyy/MM/dd}</td>
                            <td class='DeliveryTimeClass'>{DisplayBin(@item.DeliveryTimeClass)}</td>
                            <td class='DeliverySlipNumber'>{@item.DeliverySlipNumber}</td>
                            <td class='DeliveryProductNumber'>{@item.DeliveryProductNumber}</td>
                            <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                            <td class='LotNumber'>{@item.LotNumber}</td>
                            <td class='LotQuantity'>{Utils.FormatNumber(@item.LotQuantity)}</td>
                            <td class='NumberOfBoxes'>{Utils.FormatNumber(@item.NumberOfBoxes)}</td>
                            <td class='Quantity'>{Utils.FormatNumber(@item.Quantity)}</td>
                            <td class='MainProductKey'>{@item.MainProductKey}</td>
                            <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                            <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                            <td class='Remarks'>{@item.Remarks}</td>
                            <td class='CreatedAt'>{@item.CreatedAt:yyyy/MM/dd HH:mm}</td>
                            <td class='CreatedBy'>{@item.CreatedBy}</td>                        
                            <input type='hidden' class='DepoID' value='{item.DepoID}' />
                            <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                            <input type='hidden' class='SelectedBin' value='{item.SelectedBin}' />
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
        /// 出庫実績更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(D_StoreOutModel model)
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
                    _logger.Error($"出庫実績更新失敗 {errorMessage}");
                    return NotFound(new { errorMessage });
                }

                // 納入先品番で品番チェック
                var product = M_ProductConnectController.GetProductByDeliveryProductNumber(
                    model.SelectedCompanyID, model.SelectedDepoID, model.DeliveryProductNumber, user.DatabaseName);
                if (product == null)
                {
                    // log取得
                    errorMessage = errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber"));
                    _logger.Error($"出庫実績更新失敗 {errorMessage}");
                    return NotFound(new { errorMessage });
                }
                model.SupplierProductNumber = model.DeliveryProductNumber;
                model.NumberOfBoxes = (int)Math.Ceiling((double)model.Quantity / product.LotQuantity);

                // 出庫実績更新
                D_StoreOutConnectController.UpdateDStoreOut(model, user);

                // log取得
                _logger.Info($"出庫実績更新成功 出庫実績ID:{model.StoreOutID}");

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
        /// 便を表示
        /// </summary>
        public string DisplayBin(int bin)
        {
            if (bin == 0)
            {
                return string.Empty;
            }
            return bin.ToString();
        }

        /// <summary>
        /// 便リスト取得
        /// </summary>
        /// <param name="searchDeliveryDate">納入指示日</param>
        public IActionResult ChangeDeliveryTimeClassList(string searchDeliveryDate)
        {
            var searchData = string.Empty;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 便・納品書番号
                List<SelectListItem> binSelectList = D_StoreOutConnectController.GetDeliveryTimeClassList(searchDeliveryDate, user.DatabaseName);

                // 表示用のhtml作成
                if (binSelectList.Count > 0)
                {
                    searchData = string.Empty;
                    foreach (var item in binSelectList)
                    {
                        searchData += $@" <option value='{item.Text}'>{item.Text}</option>";
                    }
                }
                else
                {
                    searchData += $@" <option value='0便・'>なし</option>";
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
            var model = new D_StoreOutModel();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // メイン倉庫を初期選択
                List<D_StoreOutModel> storeInList = new();
                for (int i = 0; i < InitRegisterRowCount; i++)
                {
                    D_StoreOutModel viewModel = new()
                    {
                        SelectedDepoID = user.MainDepoID
                    };
                    storeInList.Add(viewModel);

                    model.RegisterList = storeInList;
                }

                // 納入指示日
                model.SearchDeliveryDate = Utils.GetNextday(DateTime.Today).ToString("yyyy/MM/dd");

                // 便-納品書番号
                List<SelectListItem> binSelectList = D_StoreOutConnectController.GetDeliveryTimeClassList(model.SearchDeliveryDate, user.DatabaseName);
                model.BinSelectedList = binSelectList;

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
        /// 出庫実績登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(D_StoreOutModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                List<string> errorMessageList = new();
                int registerCount = 0;
                for (int i = 0; i< model.RegisterList.Count; i++)
                {
                    var modelItem = model.RegisterList[i];
                    if (modelItem.DeliveryProductNumber.Equals("0")) continue;
                    registerCount++;

                    modelItem.SearchDeliveryDate = model.SearchDeliveryDate;
                    var validationContext = new ValidationContext(modelItem);
                    var validationResults = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(modelItem, validationContext, validationResults, true);
                    List<string> errorMembers = validationResults.SelectMany(result => result.MemberNames).Distinct().ToList();

                    // 納入先品番チェック
                    bool isContainDeliveryProductNumber = errorMembers.Contains("DeliveryProductNumber");
                    if (!isContainDeliveryProductNumber)
                    {
                        // 納入先品番で品番チェック
                        var product = M_ProductConnectController.GetProductByDeliveryProductNumber(
                            model.SelectedCompanyID, model.SelectedDepoID, modelItem.DeliveryProductNumber, user.DatabaseName
                        );
                        if (product == null)
                        {
                            isValid = false;
                            var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber"));
                            validationResults.Add(new ValidationResult(message, new List<string> { "DeliveryProductNumber" }));
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
                            List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_StoreOutModel>(err);
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
                    _logger.Error($"取込失敗");

                    return NotFound(new { errorMessage });
                }

                model.RegisterList = model.RegisterList.Where(x => !x.DeliveryProductNumber.Equals("0")).ToList();
                // 出庫実績登録
                D_StoreOutConnectController.InsertDStoreOuts(model, user);

                // log取得
                _logger.Info($"出庫実績登録成功 出庫日:{model.StoreOutDate}");

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
        /// 出庫実績削除
        /// </summary>
        /// <param name="id">出庫実績ID</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 出庫実績削除
                D_StoreOutConnectController.DeleteDStoreOut(id, user);

                // log取得
                _logger.Info($"出庫実績削除成功 出庫実績ID:{id}");

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

                // 出庫実績情報取得
                D_StoreOutModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                };
                var sql = D_StoreOutConnectController.CreateSQLToSelectDStoreOuts(model);

                List<D_StoreOutModel> searchList = D_StoreOutConnectController.ConnectDStoreOuts(sql, user.DatabaseName);
                if (searchList.Count > 0)
                {
                    foreach (D_StoreOutModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("StoreOutID")] = item.StoreOutID.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("SupplierName")] = item.SupplierName.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("StoreOutDate")] = Utils.ConvertToYYYYMMDD(item.StoreOutDate);
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("DeliveryDate")] = item.DeliveryDate.ToString("yyyy/MM/dd");
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("DeliveryTimeClass")] = item.DeliveryTimeClass.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("DeliverySlipNumber")] = item.DeliverySlipNumber.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber")] = item.DeliveryProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("LotNumber")] = item.LotNumber.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("MainProductKey")] = item.MainProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("FirstSubProductKey")] = item.FirstSubProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("SecondSubProductKey")] = item.SecondSubProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("Remarks")] = item.Remarks.ToString();
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("CreatedAt")] = item.CreatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<D_StoreOutModel>("CreatedBy")] = item.CreatedBy.ToString();

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

            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("StoreOutID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("StoreOutDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("DeliveryDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("DeliveryTimeClass"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("DeliverySlipNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("LotQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("MainProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("FirstSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("SecondSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("Remarks"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_StoreOutModel>("CreatedBy"), typeof(string));

            return table;
        }
    }
}
