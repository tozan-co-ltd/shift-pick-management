using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using ai_truck_load_measurement.ConnectControllers;
using X.PagedList;
using ai_truck_load_measurement.Commons;
using System.Data.SqlClient;
using System.Data;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 車両マスター画面
    /// </summary>
    public class M_TruckController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 車両マスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_TruckModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 車両マスター情報取得SQL作成
                var sql = M_TruckConnectController.CreateSQLToSelectMTrucks();
                // DB接続
                IEnumerable<M_TruckModel> demoList = M_TruckConnectController.ConnectMTrucks(sql, "AI-truck-load-measurement_test");

                model.M_TruckList = demoList.ToPagedList();

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
        /// 車両マスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_TruckModel model = new();
            try
            {
                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 車両マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_TruckModel model)
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
                    _logger.Error($"車両マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 車両コード重複チェック
                var sqlTruckNumber = M_TruckConnectController.CreateSQLToSelectDuplicateMTruck(model.TruckNumber);
                bool isExistedTruckNumber = ConnectToSQLServer.IsExistedSameRecord(sqlTruckNumber, "AI-truck-load-measurement_test");
                var sqlIdentifyNumber = M_TruckConnectController.CreateSQLToSelectDuplicateMTruckIdentifyNumber(model.IdentifyNumber);
                bool isExistedIdentifyNumber = ConnectToSQLServer.IsExistedSameRecord(sqlIdentifyNumber, "AI-truck-load-measurement_test");
                if (isExistedTruckNumber || isExistedIdentifyNumber)
                {
                    string displayName = "";
                    if (isExistedTruckNumber)
                    {
                        displayName = Utils.GetDisplayName<M_TruckModel>("TruckNumber");
                    }
                    else
                    {
                        displayName = Utils.GetDisplayName<M_TruckModel>("IdentifyNumber");
                    }

                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, displayName);
                    _logger.Error($"車両マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 車両マスター登録
                M_TruckConnectController.InsertMTruck(model, user);

                // log取得
                _logger.Info($"車両マスター登録成功 車両コード:{model.TruckNumber}");

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
        /// 車両マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_TruckModel model)
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
                    _logger.Error($"車両マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 異なるIDで車両コード重複チェック
                var sql = M_TruckConnectController.CreateSQLToSelectDuplicateEditMTruck(model);
                bool isExistedTruckNumber = ConnectToSQLServer.IsExistedSameRecord(sql, "AI-truck-load-measurement_test");
                var sqlIdentifyNumber = M_TruckConnectController.CreateSQLToSelectDuplicateEditMTruckIdentifyNumber(model);
                bool isExistedIdentifyNumber = ConnectToSQLServer.IsExistedSameRecord(sqlIdentifyNumber, "AI-truck-load-measurement_test");
                if (isExistedTruckNumber || isExistedIdentifyNumber)
                {
                    string displayName = "";
                    if (isExistedTruckNumber)
                    {
                        displayName = Utils.GetDisplayName<M_TruckModel>("TruckNumber");
                    }
                    else
                    {
                        displayName = Utils.GetDisplayName<M_TruckModel>("IdentifyNumber");
                    }

                    // log取得
                    errorMessage = errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, displayName);
                    _logger.Error($"車両マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 車両マスター更新
                M_TruckConnectController.UpdateMTruck(model, user);

                // log取得
                _logger.Info($"車両マスター更新成功 車両ID:{model.TruckID}");

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
        /// 車両マスター削除
        /// </summary>
        /// <param name="truckId">車両ID</param>
        /// <returns></returns>
        public IActionResult Delete(int truckId)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 車両マスター削除
                int deleteAffectedRows = M_TruckConnectController.DeleteMTruck(truckId, user);

                // log取得
                _logger.Info($"車両マスター削除成功 車両ID:{truckId}");

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
        /// <param name="gamenName">画面名</param>
        public JsonResult ExportFile(string gamenName)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // 車両マスター情報取得
                var sql = M_TruckConnectController.CreateSQLToSelectMTrucks();
                List<M_TruckModel> selectedList = M_TruckConnectController.ConnectMTrucks(sql, "AI-truck-load-measurement_test");

                // DataRowに格納
                if (selectedList.Count > 0)
                {
                    foreach (M_TruckModel item in selectedList)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_TruckModel>("TruckID")] = item.TruckID.ToString();
                        newRow[Utils.GetDisplayName<M_TruckModel>("TruckNumber")] = item.TruckNumber;
                        newRow[Utils.GetDisplayName<M_TruckModel>("IdentifyNumber")] = item.IdentifyNumber;
                        newRow[Utils.GetDisplayName<M_TruckModel>("UpdatedAt")] = item.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
                        newRow[Utils.GetDisplayName<M_TruckModel>("UpdatedBy")] = item.UpdatedBy;

                        dataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(null, gamenName);
                // CSVファイルへのパスを作成
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換
                CreateFile.ConvertDataTableToCsv(dataTable, filePath);
                // ファイル作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
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

            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("TruckID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("TruckNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("IdentifyNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedBy"), typeof(string));

            return table;
        }
    }
}
