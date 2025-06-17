using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.IO.Compression;
using X.PagedList;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ai_truck_load_measurement.Controllers
{
    public class AlertRecordController : BaseController
    {
        public IActionResult Index(int TransitionAlertRecordID, bool? TransitionIsArrived)
        {
            var model = new AlertRecordViewModel();
            try
            {

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // アラート履歴情報取得SQL作成
                var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord();
                // DB接続
                List<AlertRecordModel> alertRecordList = AlertRecordConnectController.ConnectTAlertRecords<AlertRecordModel>(alertRecordSql);

                // 便実績情報取得SQL作成
                var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                List<LoadRecordModel> loadRecordList = AlertRecordConnectController.ConnectTAlertRecords<LoadRecordModel>(loadRecordSql);

                // テーブル情報を変換
                model = ConversionForViewModel(alertRecordList, loadRecordList);

                // ログインユーザーのメインデポ情報取得
                model.MainDepoID = user.MainDepoID;
                model.MainDepoName = user.MainDepoName;
                model.TransitionAlertRecordID = TransitionAlertRecordID;
                if (TransitionIsArrived != null)
                    model.TransitionIsArrived = TransitionIsArrived.Value;
                else
                    model.TransitionIsArrived = false;
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
        /// 各種情報を表示用に変換
        /// </summary>
        /// <param name="alertRecordList"></param>
        /// <param name="loadRecordList"></param>
        /// <returns></returns>
        private AlertRecordViewModel ConversionForViewModel(List<AlertRecordModel> alertRecordList, List<LoadRecordModel> loadRecordList)
        {
            AlertRecordViewModel model = new();
            foreach (var alertRecord in alertRecordList)
            {
                // アラート履歴に対応した便実績
                var loadRecord = loadRecordList.Find(x => x.TripRecordID == alertRecord.TripRecordID)!;
                
                alertRecord.ArrivalLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(alertRecord.ArrivalLowerLoadClass);
                alertRecord.DepartureLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(alertRecord.DepartureLowerLoadClass);

                // アラート項目取得
                var alertItems = GetAlertItems(alertRecord, loadRecord);
                alertRecord.ArrivalAlertItems = alertItems.FindAll(x => x.Contains("到着"));
                alertRecord.DepartureAlertItems = alertItems.FindAll(x => x.Contains("出発"));
            }
            model.AlertRecordList = alertRecordList;
            model.LoadRecordList = loadRecordList;
            return model;
        }



        /// <summary>
        /// アラート履歴情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<AlertRecordModel> alertRecordList = new();
            List<LoadRecordModel> loadRecordList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    // アラート履歴情報取得SQL作成
                    var alertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecord(startOfPeriod, endOfPeriod, checkedDepos);
                    // DB接続
                    alertRecordList = AlertRecordConnectController.ConnectTAlertRecords<AlertRecordModel>(alertRecordSql);

                    if (alertRecordList.Count > 0)
                    {
                        // アラート履歴に対応する便実績情報取得SQL作成
                        var loadRecordSql = AlertRecordConnectController.CreateSQLToSelectTripRecordFromAlertRecord(alertRecordList);
                        // DB接続
                        loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                    }
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripTable"">
                            <thead>
                                <tr align=""center"">
                                    <th hidden>アラート履歴ID</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">便枝番</th>
                                    <th class=""font-weight-bold"">ステーション</ br>名</th>
                                    <th class=""font-weight-bold"">乗務員</th>
                                    <th class=""font-weight-bold"">アラート項目</th>
                                    <th class=""font-weight-bold"">稼働日</th>
                                    <th class=""font-weight-bold"">到着情報</th>
                                    <th class=""font-weight-bold"">出発情報</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // テーブルのbody部分
                if (alertRecordList.Count > 0)
                {
                    foreach (var alertRecord in alertRecordList)
                    {
                        // アラート履歴に対応した便実績
                        var loadRecord = loadRecordList.Find(x => x.TripRecordID == alertRecord.TripRecordID)!;
                        // アラート項目部分のhtml取得
                        var alertItems = GetAlertItems(alertRecord, loadRecord);
                        var alertItemsHTML = ConversionAlertItemsToHTML(alertItems);

                        searchData += $@"
                            <tr>
                                <td hidden>{alertRecord.AlertRecordID}</td>
                                <td>{loadRecord.TripName}</td>
                                <td>{loadRecord.TripBranchSeq}</td>
                                <td>{loadRecord.StationName}</td>
                                <td>{loadRecord.DriverName}</td>
                                <td>{alertItemsHTML}</td>
                                <td>{loadRecord.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalAlertImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnDepartureAlertImageClick('{alertRecord.AlertRecordID}', this)"" data-id=""{alertRecord.AlertRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                    <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                            </tr>
                    ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

                return Json(new SearchedTripRecordListModel
                {
                    searchedTripRecordHTML = searchData,
                    searchedTripRecordLength = alertRecordList.Count()
                });
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
        /// アラート項目取得
        /// </summary>
        /// <param name="alertRecord"></param>
        /// <param name="loadRecord"></param>
        /// <returns></returns>
        public List<string> GetAlertItems(AlertRecordModel alertRecord, LoadRecordModel loadRecord)
        {
            var alertItems = new List<string>();

            // 到着時間判定
            var arrivalTimeAlert = TimeAlertDetermination(loadRecord!.ArrivalScheduledTime, loadRecord.ArrivedAt);
            if (!string.IsNullOrEmpty(arrivalTimeAlert))
                alertItems.Add("到着時間" + arrivalTimeAlert);

            // 出発時間判定
            var departureTimeAlert = TimeAlertDetermination(loadRecord!.DepartureScheduledTime, loadRecord.DepartedAt);
            if(!string.IsNullOrEmpty(departureTimeAlert))
                alertItems.Add("出発時間" + departureTimeAlert);

            // 到着荷量判定
            if (loadRecord.ArrivalLoadClass != -1 && loadRecord.ArrivalLoadClass < alertRecord.ArrivalLowerLoadClass)
                alertItems.Add("到着荷量(下限)");

            // 出発荷量判定
            if (loadRecord.DepartureLoadClass != -1 && loadRecord.DepartureLoadClass < alertRecord.DepartureLowerLoadClass)
                alertItems.Add("出発荷量(下限)");

            return alertItems;
        }

        /// <summary>
        /// 時間アラート判定
        /// </summary>
        /// <param name="scheduledTime"></param>
        /// <param name="recordtime"></param>
        /// <returns></returns>
        private string TimeAlertDetermination(DateTime scheduledTime, DateTime recordtime)
        {
            var timeAlert = "";
            var scheduledTimeToday = new DateTime(recordtime.Year, recordtime.Month, recordtime.Day, scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);

            if (scheduledTimeToday.AddHours(-1) > recordtime)
                timeAlert = "(早)";
            else if (recordtime > scheduledTimeToday.AddHours(1))
                timeAlert = "(遅)";

            return timeAlert;
        }

        /// <summary>
        /// アラート項目のhtml取得
        /// </summary>
        /// <param name="alertItems">アラート項目リスト</param>
        /// <returns></returns>
        public string ConversionAlertItemsToHTML(List<string> alertItems)
        {
            var html = "";
            for (int i = 0; i < alertItems.Count; i++)
            {
                if (i > 0)
                    html += "<br>";
                html += alertItems[i];
            }
            return html;
        }

        /// <summary>
        /// モーダルに表示する情報取得
        /// </summary>
        /// <param name="loadRecord"></param>
        /// <returns></returns>
        public LoadRecordModel GetModalItems(LoadRecordModel loadRecord)
        {
            // 荷量のクラスから％表示に
            loadRecord.ArrivalLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(loadRecord.ArrivalLoadClass);
            loadRecord.DepartureLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(loadRecord.DepartureLoadClass);


            // 出発・到着予定時刻の日付を変換
            loadRecord.ArrivalScheduledTime = ConversionScheduledTime(loadRecord.ArrivalScheduledTime, loadRecord.ArrivedAt);
            loadRecord.DepartureScheduledTime = ConversionScheduledTime(loadRecord.DepartureScheduledTime, loadRecord.DepartedAt);

            // 画像パス変換
            loadRecord.ArrivalLoadImgPath = LoadRecordController.CheckAndConvertImagePath(loadRecord.ArrivalLoadImgPath);
            loadRecord.DepartureLoadImgPath = LoadRecordController.CheckAndConvertImagePath(loadRecord.DepartureLoadImgPath);

            return loadRecord;
        }

        /// <summary>
        /// 出発・到着予定時刻の日付を変換
        /// </summary>
        /// <param name="scheduledTime"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        private DateTime ConversionScheduledTime(DateTime scheduledTime, DateTime recordTime)
        {
            return new DateTime(recordTime.Year, recordTime.Month, recordTime.Day, scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
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


                // アラート履歴情報取得
                DataTable tAlertRecordDT = new DataTable();
                if (checkedDepos.Count > 0)
                {
                    var tAlertRecordSql = AlertRecordConnectController.CreateSQLToSelectAlertRecordForDataTable(startOfPeriod, endOfPeriod, checkedDepos);
                    tAlertRecordDT = ConnectToSQLServer.ConnectToDataTable(tAlertRecordSql);

                    // 荷量のクラスの数値化とアラート項目生成
                    tAlertRecordDT = GetConvertedLoadClassDataTable(tAlertRecordDT);
                }

                // ファイル名
                var tmpFilename = $"アラート履歴_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "検索条件シート";
                string sheetNameTwo = "荷量実績シート";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(searchConditionDT, tAlertRecordDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName);

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
        /// データテーブルの荷量クラスを数値に変換
        /// </summary>
        /// <param name="dt">変換元データテーブル</param>
        /// <returns></returns>
        public DataTable GetConvertedLoadClassDataTable(DataTable dt)
        {
            // テーブルに値を変換した後の文字列を格納する列を追加
            dt.Columns.Add("alert_items", typeof(string)).SetOrdinal(dt.Columns.IndexOf("driver_name"));
            dt.Columns.Add("arrival_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("arrival_load_class"));
            dt.Columns.Add("arrival_lower_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("arrival_lower_load_class"));
            dt.Columns.Add("departure_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("departure_load_class"));
            dt.Columns.Add("departure_lower_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("departure_lower_load_class"));

            // 各列の値を適切な値に変換
            foreach (DataRow row in dt.Rows)
            {
                // 荷量クラスを%表示に変換
                var arrivalLoadClass = (int)row["arrival_load_class"];
                var arrivalLowerLoadClass = (int)row["arrival_lower_load_class"];
                var departureLoadClass = (int)row["departure_load_class"];
                var departureLowerLoadClass = (int)row["departure_lower_load_class"];
                row["arrival_load_status"] = LoadRecordController.ConversionLoadClassToLoadStatus(arrivalLoadClass);
                row["arrival_lower_load_status"] = LoadRecordController.ConversionLoadClassToLoadStatus(arrivalLowerLoadClass);
                row["departure_load_status"] = LoadRecordController.ConversionLoadClassToLoadStatus(departureLoadClass);
                row["departure_lower_load_status"] = LoadRecordController.ConversionLoadClassToLoadStatus(departureLowerLoadClass);

                // アラート項目を生成
                AlertRecordModel alertRecord = new AlertRecordModel
                {
                    ArrivalLowerLoadClass = arrivalLowerLoadClass,
                    DepartureLowerLoadClass = departureLowerLoadClass,
                };
                LoadRecordModel loadRecord = new LoadRecordModel
                {
                    ArrivalScheduledTime = DateTime.Parse(row["arrival_scheduled_time"].ToString()!),
                    DepartureScheduledTime = DateTime.Parse(row["departure_scheduled_time"].ToString()!),
                    ArrivedAt = DateTime.Parse(row["arrived_at"].ToString()!),
                    DepartedAt = DateTime.Parse(row["departed_at"].ToString()!),
                    ArrivalLoadClass = arrivalLoadClass,
                    DepartureLoadClass = departureLoadClass
                };
                var alertItems = GetAlertItems(alertRecord, loadRecord);
                row["alert_items"] = ConvertAlertItemsToString(alertItems);
            }

            // 変換前の列を削除
            dt.Columns.Remove("arrival_load_class");
            dt.Columns.Remove("arrival_lower_load_class");
            dt.Columns.Remove("departure_load_class");
            dt.Columns.Remove("departure_lower_load_class");
            return dt;
        }

        private string ConvertAlertItemsToString(List<string> alertItems)
        {
            var alertItemString = "";
            for (int i = 0; i < alertItems.Count; i++)
            {
                if (i > 0)
                    alertItemString += ", ";

                alertItemString += alertItems[i];
            }
            return alertItemString;
        }

        /// <summary>
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download"></param>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみのデータか</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                IEnumerable<LoadRecordModel> tripRecordList = new List<LoadRecordModel>();

                if (checkedDepos.Count > 0)
                {
                    // 指定した期間の便実績情報取得SQL作成
                    var sql = AlertRecordConnectController.CreatSQLToSelectTripRecordForImage(startOfPeriod, endOfPeriod, checkedDepos);
                    // DB接続
                    tripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
                }

                var checkedDeposName = new List<LoadRecordModel>();
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
                            if (string.IsNullOrEmpty(tripBranchSeq)) tripBranchSeq = "-";


                            // ファイルネームの指定
                            var FileNameArrive = string.Format($"{tripName}_{tripBranchSeq}_{date}_A_{arrivalLoadStatus}.jpg");
                            var FileNameDeparture = string.Format($"{tripName}_{tripBranchSeq}_{date}_D_{departureLoadStatus}.jpg");

                            // 到着の画像をzipストリームに書き込む
                            var zipEntry = archive.CreateEntry(FileNameArrive, CompressionLevel.Fastest);
                            using (var zipStream = zipEntry.Open())
                            {
                                zipStream.Write(arrivalLoadImgBytes, 0, arrivalLoadImgBytes.Length);
                            }

                            // 出発の画像をzipストリームに書き込む
                            var zipEntry2 = archive.CreateEntry(FileNameDeparture, CompressionLevel.Fastest);
                            using (var zipStream = zipEntry2.Open())
                            {
                                zipStream.Write(departureLoadImgBytes, 0, departureLoadImgBytes.Length);
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
                        }
                    }


                    // メモリストリームを配列に変換してViewに渡す
                    return Json(new { data = File(ms.ToArray(), "application/zip", $"荷量画像_{startDate}-{endDate}") });
                }
            }
            // エラーメッセージ取得
            // 「ファイルが存在しません。」
            var errorMessage = ErrorMessagesResources.E9999;

            return Json(new { res = "NG", error = errorMessage });
        }

    }
}