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
    /// 入荷実績照会画面
    /// </summary>
    public class D_ReceiveController : BaseController
    {
        /// <summary>
        /// 入荷実績照会画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_ReceiveModel model = new();
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
        /// 入荷実績情報取得
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <returns></returns>
        public IActionResult SearchData(D_ReceiveModel searchModel)
        {
            D_ReceiveModel model = new();
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

                // 入荷実績情報取得SQL作成
                var sql = D_ReceiveConnectController.CreateSQLToSelectDReceives(searchModel);
                // DB接続
                List<D_ReceiveModel> searchList = D_ReceiveConnectController.ConnectDReceives(sql, user.DatabaseName);

                // 表示用のhtml作成
                if (searchList.Count > 0)
                {
                    model.DReceiveModelList = searchList.ToPagedList();

                    foreach (var item in model.DReceiveModelList)
                    {
                        searchData += $@"<tr>
                        <td>
                            <a class='btn btn-secondary btn-icon-split ml-1 mr-1'
                            onclick='OnDetailClick(this)' data-id='{item.ReceiveID}' data-toggle='modal' data-target='#detail-modal'>
                                <i class='fa-solid fa-list'></i>
                            </a>
                        </td>
                        <td class='ReceiveID'>{@item.ReceiveID}</td>
                        <td class='SupplierName'>{@item.SupplierName}</td>
                        <td class='ReceiveDatetime'>{Utils.ConvertToYYYYMMDD(@item.ReceiveDatetime)}</td>
                        <td class='SupplierProductNumber'>{@item.SupplierProductNumber}</td>
                        <td class='LotNumber'>{@item.LotNumber}</td>
                        <td class='NumberOfBoxes'>{@item.NumberOfBoxes}</td>
                        <td class='Quantity'>{@item.Quantity}</td>
                        <td class='MainProductKey'>{@item.MainProductKey}</td>
                        <td class='FirstSubProductKey'>{@item.FirstSubProductKey}</td>
                        <td class='SecondSubProductKey'>{@item.SecondSubProductKey}</td>
                        <td class='CreatedAt'>{@item.CreatedAt.ToString("yyyy/MM/dd")}</td>
                        <td class='ScanedAt'>{@item.ScanedAt}</td>
                        <td class='CreatedBy'>{@item.CreatedBy}</td>
                        <input type='hidden' class='DepoID' value='{item.DepoID}' />
                        <input type='hidden' class='DepoName' value='{item.DepoName}'/>                            
                        <input type='hidden' class='SupplierID' value='{item.SupplierID}' />
                        <input type='hidden' class='ScanResultID' value='{item.ScanResultID}'/>
                        <input type='hidden' class='HandyMenuID' value='{item.HandyMenuID}'/>
                        <input type='hidden' class='HandyMenuName' value='{item.HandyMenuName}'/>
                        <input type='hidden' class='SupplierKanbanID' value='{item.SupplierKanbanID}'/>
                        <input type='hidden' class='NumberOfInputBoxes' value='{item.NumberOfInputBoxes}'/>
                        <input type='hidden' class='FirstScanedString' value='{item.FirstScanedString}'/>
                        <input type='hidden' class='SecondScanedString' value='{item.SecondScanedString}'/>
                        <input type='hidden' class='ScanCreatedBy' value='{item.ScanCreatedBy}'/>
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

                // 入荷実績情報取得
                D_ReceiveModel model = new()
                {
                    SelectedDepoID = searchModel.DepoID,
                    SelectedCompanyID = searchModel.CompanyID,
                    SearchStartDate = searchModel.SearchStartDate,
                    SearchEndDate = searchModel.SearchEndDate,
                };
                var sql = D_ReceiveConnectController.CreateSQLToSelectDReceives(model);
                List<D_ReceiveModel> searchList = D_ReceiveConnectController.ConnectDReceives(sql, user.DatabaseName);
                
                // DataRowに格納
                if (searchList.Count > 0)
                {
                    foreach (D_ReceiveModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("ReceiveID")] = item.ReceiveID.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("SupplierName")] = item.SupplierName.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("ReceiveDatetime")] = Utils.ConvertToYYYYMMDD(item.ReceiveDatetime);
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("LotNumber")] = item.LotNumber.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("Quantity")] = item.Quantity.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("MainProductKey")] = item.MainProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("FirstSubProductKey")] = item.FirstSubProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("SecondSubProductKey")] = item.SecondSubProductKey.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("CreatedAt")] = item.CreatedAt.ToString();
                        newRow[Utils.GetDisplayName<D_ReceiveModel>("CreatedBy")] = item.CreatedBy.ToString();

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

            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("ReceiveID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("SupplierName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("ReceiveDatetime"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("MainProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("FirstSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("SecondSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveModel>("CreatedBy"), typeof(string));

            return table;
        }
    }
}
