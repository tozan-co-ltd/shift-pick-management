using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.Controllers
{
    public class NonDepartedAtRecordController : BaseController
    {
        public IActionResult Index()
        {
            NonDepartedAtRecordViewModel model = new();
            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();
            model.UserName = user.UserName;
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;
            return View(model);
        }

        /// <summary>
        /// 出発実績が存在しないデータの表作成
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            try
            {
                var sql = NonDepartedAtRecordConnectController.CreateSQLToSelectDepartedAtIsNull(startOfPeriod, endOfPeriod, checkedDepos);
                List<DepartedAtIsNullStatusModel> statuses = ConnectToSQLServer.ExecuteQueryToList<DepartedAtIsNullStatusModel>(sql);

                var html = $@"
                <div class=""mt-3"">
                    <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                        <thead>
                            <tr align=""center"">
                            <th rowspan=""2"" style=""vertical-align: middle; border-left: 1px solid #404040;"">稼働日</th>
                            <th rowspan=""2"" style=""vertical-align: middle"">出発実績無合計</th>
                ";

                foreach (var depoID in checkedDepos)
                {
                    var lastCol = "";
                    var depoName = "";
                    if (depoID == "1")
                        depoName = "SyncBace名和";
                    else if (depoID == "3")
                        depoName = "船見デポ";
                    if (depoID == checkedDepos.Last())
                        lastCol = $"style=\"border-right: 2px solid #404040;\"";
                    html += $@"<th colspan=""3"" {lastCol} >{depoName}</th> 
                    ";
                } 

                html += $@"
                            </tr>
                            <tr align=""center"">
                ";

                foreach (var depoID in checkedDepos)
                {
                    var lastCol = "";
                    if (depoID == checkedDepos.Last())
                        lastCol = $"style=\"border-right: 2px solid #404040;\"";
                    html += $@"
                            <th>ID有</th>
                            <th>ID無</th>
                            <th {lastCol} class=""trip-count-all"">稼働総数</th>
                    ";
                }

                html += $@"
                        </tr>
                        </thead>
                        <tbody>"
                ;

                for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
                {
                    var totalNullCount = 0;
                    var countHTML = "";

                    html += $@"
                            <tr>
                                <td>{date.ToString("yyyy/MM/dd")}</td>
                    ";

                    foreach (var depoName in checkedDepos)
                    {
                        var status = statuses.Find(x => x.WorkDay == date && x.DepoID.ToString() == depoName);
                        if (status != null)
                        {
                            countHTML += $@"
                                <td>{status.NullCount}</td>
                                <td>{status.NoIdentifyNumberNullCount}</td>
                                <td class=""trip-count-all"">{status.TripCount}</td>
                            ";
                            totalNullCount += status.NullCount + status.NoIdentifyNumberNullCount;
                        }
                        else
                        {
                            countHTML += $@"
                                <td>0</td>
                                <td>0</td>
                                <td class=""trip-count-all"">0</td>
                            ";
                        }
                    }

                    html += $@"
                                <td style=""font-weight: bold"">{totalNullCount}</td>
                                {countHTML}
                            </tr>";
                }

                html += $@"
                        </tbody>
                    </table>
                </div>
            ";
                return Json(html);
            }
            catch (SqlException)
            {
                return Json(new { res = "NG", errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Json(new { res = "NG", errorMessage = errorMessage });
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


                // 便実績情報取得
                DataTable tTripRecordDT = new DataTable();
                if (checkedDepos.Count > 0)
                {
                    var sql = NonDepartedAtRecordConnectController.CreateSQLToSelectDepartedAtIsNull(startOfPeriod, endOfPeriod, checkedDepos);
                    List<DepartedAtIsNullStatusModel> statuses = ConnectToSQLServer.ExecuteQueryToList<DepartedAtIsNullStatusModel>(sql);

                    // 荷量のクラスを数値化
                    tTripRecordDT = ConvertedDataTableForExcel(statuses, startOfPeriod, endOfPeriod, checkedDepos);
                }

                // 便実績が0の場合
                if (tTripRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"出発実績未登録データ_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "出発実績未登録データ";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(tTripRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, checkedDepos);

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
        /// 実績リストをエクセル用のデータテーブルに変換する
        /// </summary>
        /// <param name="statuses"></param>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public DataTable ConvertedDataTableForExcel(List<DepartedAtIsNullStatusModel> statuses, DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            DataTable convertTable = new DataTable();

            // 列の設定
            convertTable = SettingDataTableColumns(convertTable, checkedDepos);

            // 選択した期間1日ごとに
            for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
            {
                DataRow dataRow = convertTable.NewRow();
                dataRow["workDay"] = date.ToString("yyyy/MM/dd");
                var totalNullCount = 0;

                // デポごとに
                foreach (var depoID in checkedDepos)
                {
                    // 稼働日とデポIDが一致するデータを取得
                    var status = statuses.Find(x => x.WorkDay == date && x.DepoID.ToString() == depoID);

                    // データが存在する場合
                    if (status != null)
                    {
                        dataRow = SettingDataRowParameter(dataRow, depoID, status.NullCount, status.NoIdentifyNumberNullCount, status.TripCount);
                        totalNullCount += status.NullCount + status.NoIdentifyNumberNullCount;
                    }
                    else
                    {
                        dataRow = SettingDataRowParameter(dataRow, depoID, 0, 0, 0);
                    }
                }

                dataRow["totalNullCount"] = totalNullCount;
                convertTable.Rows.Add(dataRow);
            }
            return convertTable;
        }

        /// <summary>
        /// datatableの列設定
        /// </summary>
        /// <param name="table">設定先datatable</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
        private DataTable SettingDataTableColumns(DataTable table, List<string> checkedDepos)
        {
            table.Columns.Add("workDay", typeof(string));
            table.Columns.Add("totalNullCount", typeof(int));

            // デポごとに列追加
            foreach (var depoID in checkedDepos)
            {
                table.Columns.Add("nullCount" + depoID, typeof(string));
                table.Columns.Add("noIdentifyNumberNullCount" + depoID, typeof(string));
                table.Columns.Add("tripCount" + depoID, typeof(string));
            }
            return table;
        }

        /// <summary>
        /// datatableの行の設定
        /// </summary>
        /// <param name="dataRow">設定先のdatarow</param>
        /// <param name="depoID">デポID</param>
        /// <param name="nullCount">識別番号有、出発実績無のデータ数</param>
        /// <param name="noIdentifyNumberNullCount">識別番号無、出発実績無のデータ数</param>
        /// <param name="tripCount">便実績総数</param>
        /// <returns></returns>
        private DataRow SettingDataRowParameter(DataRow dataRow, string depoID, int nullCount, int noIdentifyNumberNullCount, int tripCount)
        {
            dataRow["nullCount" + depoID] = nullCount;
            dataRow["noIdentifyNumberNullCount" + depoID] = noIdentifyNumberNullCount;
            dataRow["tripCount" + depoID] = tripCount;
            return dataRow;
        }
    }
}
