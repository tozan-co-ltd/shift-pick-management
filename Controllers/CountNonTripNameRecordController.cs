using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace ai_truck_load_measurement.Controllers
{
    public class CountNonTripNameRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new CountNonTripNameRecordViewModel();
            var user = ClaimsLoginUserData();
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;
            model.UserName = user.UserName;
            return View(model);
        }

        /// <summary>
        /// テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<CountNonTripNameRecordModel> countNonTripNameRecordList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    var sql = CountNonTripNameRecordConnectController.CreateSQLToSelectCountNonTripNameRecord(startOfPeriod, endOfPeriod, checkedDepos);
                    countNonTripNameRecordList = ConnectToSQLServer.ExecuteQueryToList<CountNonTripNameRecordModel>(sql);
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">便ID</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">紐付け切れ回数</th>
                                    <th class=""font-weight-bold"">実績総数</th>
                                    <th class=""font-weight-bold"">デポ名</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // テーブルのbody部分
                if (countNonTripNameRecordList.Count > 0)
                {
                    foreach (var countNonTripNameRecord in countNonTripNameRecordList)
                    {
                        searchData += $@"
                            <tr>
                                <td>{countNonTripNameRecord.TripID}</td>
                                <td>{countNonTripNameRecord.TripName}</td>
                                <td>{countNonTripNameRecord.NoNameCount}</td>
                                <td>{countNonTripNameRecord.TripCount}</td>
                                <td>{countNonTripNameRecord.DepoName}</td>
                            </tr>
                        ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

                return Json(searchData);
            }
            catch (SqlException)
            {
                return Json(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Json(errorMessage);
            }
        }
        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            string? errorMessage;
            string startDate = startOfPeriod.ToString("yyyyMMdd");
            string endDate = endOfPeriod.ToString("yyyyMMdd");
            try
            {
                // 検索条件シート用データテーブル作成
                DataTable searchConditionDT = new DataTable();
                searchConditionDT.Columns.Add("項目名");
                searchConditionDT.Columns.Add("検索条件");
                // デポの設定
                var selectedDeposName = LoadRecordController.SelectedDepos(checkedDepos);
                searchConditionDT.Rows.Add("対象デポ", selectedDeposName);
                // 稼働日の設定
                searchConditionDT.Rows.Add("稼働日", $"{startDate}～{endDate}");

                var countNonTripNameRecordDT = new DataTable();

                if (checkedDepos.Count > 0)
                {
                    var sql = CountNonTripNameRecordConnectController.CreateSQLToSelectCountNonTripNameRecord(startOfPeriod, endOfPeriod, checkedDepos);
                    countNonTripNameRecordDT = ConnectToSQLServer.ConnectToDataTable(sql);
                }


                // 便実績が0の場合
                if (countNonTripNameRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"紐づけ切れ回数_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "紐づけ切れ回数";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(countNonTripNameRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, checkedDepos);

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
    }
}
