using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出荷実績照会画面
    /// </summary>
    public class D_ShipmentController : BaseController
    {
        /// <summary>
        /// 出荷実績照会画面表示
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="categoryTitle">カテゴリータイトル</param>
        /// <returns></returns>
        public IActionResult Index(int shipmentScheduleId, int depoId, int companyId, string categoryTitle)
        {
            D_ShipmentModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫IDから倉庫情報を取得
                var searchDepoSql = M_DepoConnectController.CreateSQLToSelectByDepoId(depoId);
                List<M_DepoModel> depoSearchList = M_DepoConnectController.ConnectMDepos(searchDepoSql, user.DatabaseName);

                // 会社IDから会社情報を取得
                var searchCompanySql = M_CompanyConnectController.CreateSQLToSelectByCompanyId(companyId);
                List<M_CompanyModel> companySearchList = M_CompanyConnectController.ConnectMCompanys(searchCompanySql, user.DatabaseName);

                // 出荷実績情報取得
                string sql = D_ShipmentConnectController.CreateSQLToSelectDShipments(
                    shipmentScheduleId, depoId, companyId);
                List<D_ShipmentModel> dShipmentList = D_ShipmentConnectController.ConnectDShipments(sql, user.DatabaseName);

                model.CategoryTitle = categoryTitle;
                model.DeliveryID = companyId;
                model.DepoID = depoId;
                model.ShipmentScheduleID = shipmentScheduleId;
                model.DShipmentList = dShipmentList;
                model.DepoName = depoSearchList[0].DepoName;
                model.DeliveryName = string.Concat(companySearchList[0].CompanyName, " - ", companySearchList[0].ClientName);
                if (dShipmentList.Count > 0)
                {
                    model.DeliveryDate = dShipmentList[0].DeliveryDate;
                }

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

                if (searchModel.SearchStartDate.Equals(string.Empty))
                {
                    throw new Exception();
                }
                // 出荷実績情報取得
                string sql = D_ShipmentConnectController.CreateSQLToSelectDShipments(
                    searchModel.ShipmentScheduleID, searchModel.DepoID, searchModel.CompanyID);
                List<D_ShipmentModel> searchList = D_ShipmentConnectController.ConnectDShipments(sql, user.DatabaseName);

                // DataRowに格納
                if (searchList.Count > 0)
                {
                    foreach (D_ShipmentModel item in searchList)
                    {
                        DataRow newRow = searchResult.NewRow();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("ShipmentID")] = item.ShipmentID.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("DeliveryName")] = item.DeliveryName.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("DeliveryDate")] = item.DeliveryDate.ToString("yyyy/MM/dd");
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("DeliveryTimeClass")] = item.DeliveryTimeClass.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("ShipmentDatetime")] = item.ShipmentDatetime.ToString("yyyy/MM/dd");
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("DeliveryProductNumber")] = item.DeliveryProductNumber.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("DeliveryProductAbbreviation")] = item.DeliveryProductAbbreviation.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("KanbanSerialNumber")] = item.KanbanSerialNumber.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("SupplierProductNumber")] = item.SupplierProductNumber.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("LotNumber")] = item.LotNumber.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("NumberOfBoxes")] = item.NumberOfBoxes.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("Quantity")] = item.Quantity.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("MainProductKey")] = item.MainProductKey.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("FirstSubProductKey")] = item.FirstSubProductKey.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("SecondSubProductKey")] = item.SecondSubProductKey.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("ScanedAt")] = item.ScanedAt.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("CreatedAt")] = item.CreatedAt.ToString().Trim();
                        newRow[Utils.GetDisplayName<D_ShipmentModel>("CreatedBy")] = item.CreatedBy.ToString().Trim();

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

            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("ShipmentID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("DeliveryName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("DeliveryDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("DeliveryTimeClass"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("ShipmentDatetime"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("DeliveryProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("DeliveryProductAbbreviation"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("KanbanSerialNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("NumberOfBoxes"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("Quantity"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("MainProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("FirstSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("SecondSubProductKey"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("ScanedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ShipmentModel>("CreatedBy"), typeof(string));

            return table;
        }
    }
}
