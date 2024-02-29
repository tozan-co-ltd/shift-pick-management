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
                        searchData += "<tr>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutDate + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "</tr>";
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

                var model = new D_StoreOutModel();
                List<D_StoreOutModel> storeInList = new();
                for (int i = 0; i < InitRegisterRowCount; i++)
                {
                    var viewModel = new D_StoreOutModel();
                    viewModel.SelectedDepoID = user.MainDepoID;
                    storeInList.Add(viewModel);

                    model.RegisterList = storeInList;
                }
                // 便-納品書番号
                List<SelectListItem> binSelectList = new()
                {
                    new() { Value = "1", Text = "1 - Y001", Selected = false },
                    new() { Value = "2", Text = "2 - T001", Selected = false },
                    new() { Value = "3", Text = "3 - Z001", Selected = false }
                };
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
    }
}
