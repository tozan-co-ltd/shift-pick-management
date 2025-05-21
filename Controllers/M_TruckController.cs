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
                IEnumerable<M_TruckModel> truckList = ConnectToSQLServer.ExecuteQueryToList<M_TruckModel>(sql);

                model.M_TruckList = truckList.ToPagedList();

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
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"車両マスター登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 車両コード重複チェック
                var duplicateCheck = TrackCodeDuplicateCheck(model);
                if (duplicateCheck != null)
                {
                    errorMessage = duplicateCheck;

                    return NotFound(new { errorMessage });
                }

                // 車両マスター登録
                var sql = M_TruckConnectController.CreateSQLToInsertMTruck(model, DateTime.Now, user.UserName);
                ConnectToSQLServer.ExecuteQuery(sql);

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
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"車両マスター更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 車両コード重複チェック
                var duplicateCheck = TrackCodeDuplicateCheck(model);
                if (duplicateCheck != null)
                {
                    errorMessage = duplicateCheck;

                    return NotFound(new { errorMessage });
                }

                // 車両マスター更新
                var updateSql = M_TruckConnectController.CreateSQLToUpdateMTruck(model, DateTime.Now, user.UserName);
                ConnectToSQLServer.ExecuteQuery(updateSql);

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

                var sql = M_TruckConnectController.CreateSQLToDeleteMTruck(truckId, DateTime.Now, user.UserName);
                // 車両マスター削除
                int deleteAffectedRows = ConnectToSQLServer.ExecuteQuery(sql);

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
        /// <param name="gamenName">現在の画面名</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName)
        {
            string? errorMessage;
            try
            { 
                // 車両マスター情報取得
                var sql = M_TruckConnectController.CreateSQLToSelectMTrucksForDataTable();
                DataTable dt = ConnectToSQLServer.ConnectToDataTable(sql);

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(gamenName);
                // 2シートあり
                bool sheetTwo = false;


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(dt, null, tmpFilename, sheetTwo, null, null, gamenName);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);

                        CreateFile.DeleteFile(tmpFilename);

                        return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
                    }
                    else
                    {
                        // エラーメッセージ取得
                        // 「ファイルが存在しません。」
                        errorMessage = ErrorMessagesResources.E9999;

                        return Json(new { res = "NG", error = errorMessage });
                    }
                }
                catch (Exception ex)
                {
                    // エラーメッセージ取得
                    // 「NASに接続できませんでした。」
                    errorMessage = ErrorMessagesResources.E9999;

                    // log取得
                    var exceptionMessage = ex.Message;
                    return Json(new { res = "NG", error = errorMessage + exceptionMessage });
                }
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;

                // log取得
                var exceptionMessage = ex.Message;
                return Json(new { res = "NG", error = errorMessage + exceptionMessage });
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
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("IsDeleted"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("CreatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("CreatedBy"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedBy"), typeof(string));

            return table;
        }

        /// <summary>
        /// 車両コード重複チェック
        /// </summary>
        /// <param name="model">チェック対象</param>
        /// <returns></returns>
        private string? TrackCodeDuplicateCheck(M_TruckModel model)
        {
                var sqlTruckNumber = M_TruckConnectController.CreateSQLToSelectDuplicateMTruck(model);
                bool isExistedTruckNumber = ConnectToSQLServer.IsExistedSameRecord(sqlTruckNumber);
                var sqlIdentifyNumber = M_TruckConnectController.CreateSQLToSelectDuplicateMTruckIdentifyNumber(model);
                bool isExistedIdentifyNumber = ConnectToSQLServer.IsExistedSameRecord(sqlIdentifyNumber);
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
                    var errorMessage = "E1010: " + string.Format(ErrorMessagesResources.E1010, displayName);
                    _logger.Error($"車両マスター登録失敗 {errorMessage}");

                    return errorMessage;
                }

            return null;
        }
    }
}
