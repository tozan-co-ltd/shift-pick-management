using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出庫実績照会・修正画面
    /// </summary>
    public class D_StoreOutController : BaseController
    {
        // 新規作成行数
        private const int InitRegisterRowCount = 5;

        /// <summary>
        /// 出庫実績照会・修正画面表示
        /// </summary>
        /// <param name="model"></param>
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

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 出庫実績更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(D_StoreOutModel model)
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

                // 納入先品番で品番チェック
                bool isExistDeliveryProduct = M_ProductConnectController.IsExistedDeliveryProductNumber(model.DeliveryProductNumber, user.DatabaseName);
                if (!isExistDeliveryProduct)
                {
                    var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber"));
                    return NotFound(new { errorMessage = message });
                }

                // 仕入先品番チェック
                bool isExistProduct = M_ProductConnectController.IsExistedSupplierProductNumber(model.SupplierProductNumber, user.DatabaseName);
                if (!isExistProduct)
                {
                    var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("SupplierProductNumber"));
                    return NotFound(new { errorMessage = message });
                }

                // 出庫実績更新
                D_StoreOutConnectController.EditDStoreOut(model, user);

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
        /// 検索ボタン押下
        /// </summary>
        /// <param name="model"></param>
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
                ModelState.Remove("SupplierProductNumber");
                ModelState.Remove("DeliveryProductNumber");
                ModelState.Remove("SearchDeliveryDate");
                if (!ModelState.IsValid)
                {
                    return NotFound(new { errorMessage = "E1017: " + ErrorMessagesResources.E1017 });
                }

                // SQL作成
                var sql = D_StoreOutConnectController.CreateSQLToSelectDStoreOuts(searchModel);

                // DB接続
                List<D_StoreOutModel> dStoreOutList = D_StoreOutConnectController.ConnectDStoreOuts(sql, ClaimsLoginUserData().DatabaseName);

                // 表示用のhtml作成
                if (dStoreOutList.Count > 0)
                {
                    IEnumerable<D_StoreOutModel> query = dStoreOutList.Select(s => s);
                    model.D_StoreOutList = query.ToPagedList();

                    foreach (var item in model.D_StoreOutList)
                    {
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-success btn-icon-split ml-1 mr-1'
                            onclick='OnEditClick(this)' data-id='{item.StoreOutID}' data-toggle='modal' data-target='#edit-modal'>
                                <i class='fa-solid fa-pen'></i>
                            </a>
                            <button class='btn btn-danger btn-icon-split'
                            onclick='OnDeleteClick(this)' data-id='{item.StoreOutID}' data-toggle='modal' data-target='#delete-modal'>
                                <i class='fa-solid fa-trash'></i>
                            </button>
                        </td>
                        <td class='StoreOutID'>{@item.StoreOutID}</td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='StoreOutDate'>{@item.StoreOutDate.ToString("yyyy/MM/dd")}</td>
                        <td class='DeliveryDate'>{@item.DeliveryDate.ToString("yyyy/MM/dd")}</td>
                        <td class='DeliveryTimeClass'>{@item.DeliveryTimeClass}</td>
                        <td class='DeliverySlipNumber'>{@item.DeliverySlipNumber}</td>
                        <td class='DeliveryProductNumber'>{@item.DeliveryProductNumber}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotNumber'>{@item.LotNumber}</td>
                        <td class='LotQuantity'>{@item.LotQuantity}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='MainProductKey'>{@item.MainProductKey}</td>
                        <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                        <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                        <td class='Remarks'>{@item.Remarks}</td>
                        <td class='CreatedAt'>{@item.CreatedAt}</td>
                        <td class='CreatedBy'>{@item.CreatedBy}</td>                        
                        <input type='hidden' class='DepoID' value='{item.DepoID}' />
                        <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                        <input type='hidden' class='SelectedBin' value='{item.SelectedBin}' />
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
        /// 便リスト取得
        /// </summary>
        /// <param name="searchDeliveryDate">納入指示日</param>
        public IActionResult ChangDeliveryTimeClassList(string searchDeliveryDate)
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
                    //searchData = "<select>";
                    foreach (var item in binSelectList)
                    {
                        searchData += $@" <option value='{item.Text}'>{item.Text}</option>";
                    }
                    //searchData += "</select>";
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

                var model = new D_StoreOutModel();
                List<D_StoreOutModel> storeInList = new();
                for (int i = 0; i < InitRegisterRowCount; i++)
                {
                    var viewModel = new D_StoreOutModel();
                    viewModel.SelectedDepoID = user.MainDepoID;
                    storeInList.Add(viewModel);

                    model.RegisterList = storeInList;
                }

                // 納入指示日
                model.SearchDeliveryDate = Utils.GetNextWeekday(DateTime.Today).ToString("yyyy/MM/dd");

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
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                List<string> errorMessageList = new();
                int readCount = 1;
                // リストチェック
                if (model.RegisterList != null && model.RegisterList.Count > 0)
                {
                    foreach (var modelItem in model.RegisterList)
                    {
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
                            bool isExistProduct = M_ProductConnectController.IsExistedDeliveryProductNumber(modelItem.DeliveryProductNumber, user.DatabaseName);
                            if (!isExistProduct)
                            {
                                isValid = false;
                                var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("DeliveryProductNumber"));
                                validationResults.Add(new ValidationResult(message, new List<string> { "DeliveryProductNumber" }));
                            }
                        }

                        // 仕入先品番チェック
                        bool isContainSupplierProductNumber = errorMembers.Contains("SupplierProductNumber");
                        if (!isContainSupplierProductNumber)
                        {
                            // 仕入先品番で品番チェック
                            bool isExistProduct = M_ProductConnectController.IsExistedSupplierProductNumber(modelItem.SupplierProductNumber, user.DatabaseName);
                            if (!isExistProduct)
                            {
                                isValid = false;
                                var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_StoreOutModel>("SupplierProductNumber"));
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

                // 出庫実績登録
                D_StoreOutConnectController.InsertDStoreOuts(model, user);

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
        /// 出庫実績削除
        /// </summary>
        /// <param name="storeInId">出庫実績ID</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 出庫実績削除
                D_StoreOutConnectController.DeleteDStoreOut(id, user.DatabaseName);

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
    }
}
