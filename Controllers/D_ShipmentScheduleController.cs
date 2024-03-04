using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data;
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
                // 翌日(土日を除く)
                DateTime currentDate = DateTime.Now;
                var nextDay = Utils.GetNextWeekday(currentDate).ToString("yyyy/MM/dd");
                model.SearchStartDate = nextDay;
                model.SearchEndDate = nextDay;

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
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='DeliveryDate'>{Utils.ConvertToYYYYMMDD(@item.DeliveryDate)}</td>
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
                        <td class='IssuedDate'>{Utils.ConvertToYYYYMMDD(@item.IssuedDate)}</td>
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

                // 入荷予定情報取得
                D_ShipmentScheduleSearchModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                };
                var sql = D_ShipmentScheduleConnectController.CreateSQLToSelectDShipmentSchedules(model);

                List<D_ShipmentScheduleModel> searchList = D_ShipmentScheduleConnectController.ConnectDShipmentSchedules(sql, user.DatabaseName);
                if (searchList.Count > 0)
                {
                    foreach (D_ShipmentScheduleModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipmentScheduleID")] = item.ShipmentScheduleID.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("SupplierName")] = item.SupplierName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryDate")] = item.DeliveryDate.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTimeClass")] = item.DeliveryTimeClass.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductNumber")] = item.DeliveryProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("LotQuantity")] = item.LotQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("StoreOutNumberOfBoxes")] = item.StoreOutNumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("StoreOutQuantity")] = item.StoreOutQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererCode")] = item.OrdererCode.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryKubun")] = item.OrdererFactoryKubun.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererName")] = item.OrdererName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryName")] = item.OrdererFactoryName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperCode")] = item.ShipperCode.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperFactoryKubun")] = item.ShipperFactoryKubun.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperName")] = item.ShipperName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryCode")] = item.DeliveryCode.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryKubun")] = item.DeliveryFactoryKubun.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryLocation")] = item.DeliveryLocation.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryName")] = item.DeliveryName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryName")] = item.DeliveryFactoryName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("RegularKubun")] = item.RegularKubun.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("IssuedDate")] = item.IssuedDate.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTime")] = item.DeliveryTime.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("TranspotationIdentify")] = item.TranspotationIdentify.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipNumber")] = item.DeliverySlipNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipPageNumber")] = item.DeliverySlipPageNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipRowNumber")] = item.DeliverySlipRowNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductAbbreviation")] = item.DeliveryProductAbbreviation.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductName")] = item.DeliveryProductName.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("BranchNumber")] = item.BranchNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("UpdatedAt")] = item.UpdatedAt.ToString();
                        newRow[Utils.GetDisplayName<D_ShipmentScheduleModel>("UpdatedBy")] = item.UpdatedBy.ToString();

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

            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipmentScheduleID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTimeClass"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("LotQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("StoreOutNumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("StoreOutQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperFactoryKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryLocation"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("RegularKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("IssuedDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTime"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("TranspotationIdentify"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipPageNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipRowNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductAbbreviation"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("BranchNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentScheduleModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
