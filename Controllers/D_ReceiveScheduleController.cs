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
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
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
                        <td class='CreatedAt'>{@item.CreatedAt:yyyy/MM/dd HH:mm:ss}</td>
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
                D_ReceiveScheduleModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                    DiffenceCountCheck = searchModel.DiffenceCountCheck,
                };
                var sql = D_ReceiveScheduleConnectController.CreateSQLToSelectDReceiveSchedules(model);
                List<D_ReceiveScheduleModel> searchList = D_ReceiveScheduleConnectController.ConnectDReceiveSchedules(sql, user.DatabaseName);

                // DataRowに格納
                if (searchList.Count > 0)
                {
                    foreach (D_ReceiveScheduleModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleID")] = item.ReceiveScheduleID.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierName")] = item.SupplierName.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleDate")] = Utils.ConvertToYYYYMMDD(item.ReceiveScheduleDate);
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("LotNumber")] = item.LotNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("StoreInNumberOfBox")] = item.StoreInNumberOfBox.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("StoreInQuantity")] = item.StoreInQuantity.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("CreatedAt")] = item.CreatedAt.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveScheduleModel>("CreatedBy")] = item.CreatedBy.ToString();

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

            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("StoreInNumberOfBox"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("StoreInQuantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("CreatedBy"), typeof(string));

            return table;
        }
    }
}
