using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using X.PagedList;
using System.Drawing;
using System.Data.SqlClient;
using ai_truck_load_measurement.Commons;
using System.Data;
using System.Formats.Asn1;
using System.IO.Compression;
using System.Text;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadOutputController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            // 戻り値
            LoadRecordViewModel model = new();

            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();
            // メインデポ情報取得
            model = LoadRecordController.SetMainDepoInfo(model, user);
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList =ConnectToSQLServer.ExecuteQueryToList<LoadOutputModel>(sql);
                // テーブル情報を変換
                tripRecordList = (IEnumerable<LoadOutputModel>)LoadRecordController.ConversionForTable(tripRecordList);

                model.TripRecordList = tripRecordList.ToPagedList();
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
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
        {
            SearchedTripRecordListModel searchedTripRecordListModel = new();
            try
            {
                if (checkedDepos.Count == 0)
                {
                    return Json(searchedTripRecordListModel);
                }

                // 指定した期間の便マスター情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecordFromPeriod(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber, checkedDepos);
                searchedTripRecordListModel = LoadRecordController.SearchData(sql, "LoadOutput");

                return Json(searchedTripRecordListModel);
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

        public JsonResult SearchPivotData(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos, PivotStatusModel statuses)
        {
            var searchData = string.Empty;
            SearchedTripRecordListModel searchedTripRecordListModel = new();
            try
            {
                if (checkedDepos.Count == 0)
                {
                    return Json(searchedTripRecordListModel);
                }

                var sql = LoadOutputConnectController.CreateSQLToSelectTripRecordForDataTable(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber, checkedDepos);
                var datatable = ConnectToSQLServer.ConnectToDataTable(sql);
                var resultTable = new DataTable();
                var headers = statuses.HeaderColumuns;
                if(!headers.Contains(statuses.YColumnName))
                    headers.Add(statuses.YColumnName);
                foreach(var header in headers)
                {
                    resultTable.Columns.Add(header, datatable.Columns[header].DataType);
                }

                var headersIdx = headers.Select(i => datatable.Columns.IndexOf(i)).ToArray();
                var xColumnIdx = statuses.XColumnNames.Select(i => datatable.Columns.IndexOf(i)).ToArray();

                Dictionary<object, DataRow> dict = new Dictionary<object, DataRow>();

                // 入力のDataTableの行を反復処理します
                foreach (DataRow dr in datatable.Rows)
                {
                    // yColumnName、xColumnNames、およびvalueColumnNameの値を取得します
                    var y = dr[statuses.YColumnName];
                    var x = string.Join("_", xColumnIdx.Select(i => dr[i].ToString()).ToArray());
                    var val = dr[statuses.ValueColumnNames].ToString();

                    // 新しい列をresultTableに追加します（存在しない場合）
                    if (!resultTable.Columns.Contains(x))
                    {
                        resultTable.Columns.Add(x);
                    }

                    // 現在のy値のためのDataRowを取得または作成します
                    if (!dict.TryGetValue(y, out DataRow? row))
                    {
                        row = resultTable.NewRow();
                        row.ItemArray = headersIdx.Select(i => dr[i]).ToArray();
                        resultTable.Rows.Add(row);
                        dict[y] = row;
                    }

                    // ピボットテーブル内のyとxの交差点に値を設定します
                    row[x] = val;
                }

                var html = CreatePivotTable(resultTable);
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

        public string CreatePivotTable(DataTable dataTable)
        {
            var html = $@"
                <div class=""mt-3"">
                    <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                        <thead>
                            <tr align=""center"">"
            ;

            foreach (DataColumn col in dataTable.Columns)
            {
                html += $"<th>{col.ColumnName}</th>";
            }

            html += $@"
                            </tr>
                        </thead>
                        <tbody>"
            ;

            foreach (DataRow row in dataTable.Rows)
            {
                html += " <tr>";
                foreach (DataColumn col in row.ItemArray)
                { 
                    html += $"<td>{col.ColumnName}</td>";
                }
                html += "</tr>";
            }
            html += $@"
                        </tbody>
                    </table>
                </div>
            ";
            return html;
        }

        /// <summary>
        /// 出発実績
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public JsonResult SearchDepartedAtIsNullDatas(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            try
            {
                var sql = LoadOutputConnectController.CreateSQLToSelectDepartedAtIsNull(startOfPeriod, endOfPeriod, checkedDepos);
                List<DepartedAtIsNullStatusModel> statuses = ConnectToSQLServer.ExecuteQueryToList<DepartedAtIsNullStatusModel>(sql);

                var html = $@"
                <div class=""mt-3"">
                    <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                        <thead>
                            <tr align=""center"">
                            <th>稼働日</th>"
               ;

                foreach (var depoID in checkedDepos)
                {
                    var depoName = "";
                    if (depoID == "1")
                        depoName = "SyncBase名和北";
                    else if (depoID == "3")
                        depoName = "船見デポ";

                    html += $@"<th>{depoName} 識別番号有</th>
                        <th>{depoName} 識別番号無</th>"
                    ;
                }

                html += $@"
                            <th>当日合計</th>
                            </tr>
                        </thead>
                        <tbody>"
                ;

                for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
                {
                    var totalNullCount = 0;

                    html += $@"
                            <tr>
                                <td>{date.ToString("yyyy/MM/dd")}</td>
                ";

                    foreach (var depoName in checkedDepos)
                    {
                        var status = statuses.Find(x => x.WorkDay == date && x.DepoID.ToString() == depoName);
                        if (status != null)
                        {
                            html += $@"
                                <td>{status.NullCount}</td>
                                <td>{status.NoIdentifyNumberNullCount}</td>
                            ";
                            totalNullCount += status.NullCount + status.NoIdentifyNumberNullCount;
                        }
                        else
                        {
                            html += $@"
                                <td>0</td>
                                <td>0</td>
                        ";
                        }
                    }

                    html += $@"
                                <td>{totalNullCount}</td>
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
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
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
                searchConditionDT.Rows.Add("稼働日",$"{startDate}～{endDate}");


                // 絞り込み条件作成
                var shiborikomiCondition = ShiborikomiCondition(isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
                searchConditionDT.Rows.Add("絞り込み条件：",shiborikomiCondition);

                // 便実績情報取得
                DataTable tTripRecordDT = new DataTable();
                if (checkedDepos.Count > 0)
                {
                    var tTripRecordSql = LoadOutputConnectController.CreateSQLToSelectTripRecordForDataTable(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber, checkedDepos);
                    tTripRecordDT = ConnectToSQLServer.ConnectToDataTable(tTripRecordSql);

                    // 荷量のクラスを数値化
                    tTripRecordDT = LoadRecordController.GetConvertedLoadClassDataTable(tTripRecordDT);
                }

                // 便実績が0の場合
                if (tTripRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"荷量実績_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "荷量実績";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(tTripRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName);

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
        /// 絞り込み条件作成
        /// </summary>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみか</param>
        /// <param name="hasTripName">便名称ありのみか</param>
        /// <param name="hasIdentifyNumber">識別番号ありのみか</param>
        /// <returns></returns>
        private string ShiborikomiCondition(bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber)
        {
            var shiborikomiCondition = "";
            var hasCondition = false;
            
            if (isOnlyHasAmountDefference)
            {
                shiborikomiCondition += "荷量の相違ありのみ";
                hasCondition = true;
            }
            if (hasTripName)
            {
                if (!string.IsNullOrEmpty(shiborikomiCondition))
                {
                    shiborikomiCondition += ",";
                }
                shiborikomiCondition += "便名称ありのみ";
                hasCondition = true;
            }
            if (hasIdentifyNumber)
            {
                if (!string.IsNullOrEmpty(shiborikomiCondition))
                {
                    shiborikomiCondition += ",";
                }
                shiborikomiCondition += "識別番号ありのみ";
                hasCondition = true;
            }
            if (!hasCondition)
            {
                shiborikomiCondition = "なし";
            }
            return shiborikomiCondition;
        }


        /// <summary>
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download"></param>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみのデータか</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                IEnumerable<LoadOutputModel> tripRecordList = new List<LoadOutputModel>();

                if(checkedDepos.Count > 0)
                {
                    // 指定した期間の便実績情報取得SQL作成
                    var sql = LoadOutputConnectController.CreatSQLToSelectTripRecordForImage(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber, checkedDepos);
                    // DB接続
                    tripRecordList =ConnectToSQLServer.ExecuteQueryToList<LoadOutputModel>(sql);
                }

                var checkedDeposName =  new List<LoadRecordModel>();
                // デポ名リスト作成
                if (checkedDepos.Count > 0)
                {
                    var deposNameSQL = LoadRecordConnectController.CreateSQLToSelectDepoNameFromDepoID(checkedDepos);
                    checkedDeposName = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(deposNameSQL);
                }

                var startDate = startOfPeriod.ToString("yyyyMMdd");
                var endDate = endOfPeriod.ToString("yyyyMMdd");

                // 空のメモリストリームを生成
                using (var ms = new MemoryStream())
                {// メモリストリームを指定してZipArchiveを作成
                    using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        var date = string.Empty;
                        foreach (var tripRecord in tripRecordList)
                        {
                            // ステーションの画像取得
                            var arrivalLoadImgPath = LoadRecordController.CheckAndConvertImagePath(tripRecord.ArrivalLoadImgPath);
                            var departureLoadImgPath = LoadRecordController.CheckAndConvertImagePath(tripRecord.DepartureLoadImgPath);
                            byte[] arrivalLoadImgBytes = Convert.FromBase64String(arrivalLoadImgPath);
                            byte[] departureLoadImgBytes = Convert.FromBase64String(departureLoadImgPath);
                            // 荷量取得
                            var arrivalLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(tripRecord.ArrivalLoadClass);
                            var departureLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(tripRecord.DepartureLoadClass);
                            date = tripRecord.WorkDay.ToString("yyyyMMdd");
                            // 便名称と便枝番が空欄の時の処理
                            var tripName = tripRecord.TripName;
                            if (string.IsNullOrEmpty(tripName)) tripName = "-";
                            var tripBranchSeq = tripRecord.TripBranchSeq;
                            if(string.IsNullOrEmpty(tripBranchSeq)) tripBranchSeq = "-";

                        
                                // ファイルネームの指定
                            var FileNameArrive = string.Format($"{tripName}_{tripBranchSeq}_{date}_A_{arrivalLoadStatus}.jpg");
                            var FileNameDeparture = string.Format($"{tripName}_{tripBranchSeq}_{date}_D_{departureLoadStatus}.jpg");

                            // 到着の画像をzipストリームに書き込む
                            if (!isOnlyHasAmountDefference || tripRecord.ArrivalDepartureClass == "arrival")
                            {
                                var zipEntry = archive.CreateEntry(FileNameArrive, CompressionLevel.Fastest);
                                using (var zipStream = zipEntry.Open())
                                {
                                    zipStream.Write(arrivalLoadImgBytes, 0, arrivalLoadImgBytes.Length);
                                }
                            }
                            
                            // 出発の画像をzipストリームに書き込む
                            if(!isOnlyHasAmountDefference || tripRecord.ArrivalDepartureClass == "departure")
                            {
                                var zipEntry2 = archive.CreateEntry(FileNameDeparture, CompressionLevel.Fastest);
                                using (var zipStream = zipEntry2.Open())
                                {
                                    zipStream.Write(departureLoadImgBytes, 0, departureLoadImgBytes.Length);
                                }
                            }
                        }
                        // 検索条件のテキストファイルを追加
                        var zipEntryText = archive.CreateEntry("検索条件.txt"); 
                        using (StreamWriter sw = new StreamWriter(zipEntryText.Open(),
                            System.Text.Encoding.GetEncoding("shift_jis")))
                        {
                            //書き込む
                            sw.WriteLine($"稼働日：{startDate}～{endDate}");
                            var selectedDepos = LoadRecordController.SelectedDepos(checkedDepos);
                            sw.WriteLine($"対象デポ：{selectedDepos}");
                            var shiborikomiConditioin = ShiborikomiCondition(isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
                            sw.Write($"絞り込み条件：{shiborikomiConditioin}");
                        }
                    }
                    

                    // メモリストリームを配列に変換してViewに渡す
                    return Json(new { data = File(ms.ToArray(), "application/zip", $"荷量画像_{startDate}-{endDate}")});
                }
            }
            // エラーメッセージ取得
            // 「ファイルが存在しません。」
            var errorMessage = ErrorMessagesResources.E9999;

            return Json(new { res = "NG", error = errorMessage });
        }
    }
}

