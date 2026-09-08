using shift_pick_management.Commons;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Data;
using X.PagedList;

namespace shift_pick_management.Controllers
{
    public class NonIdentifyNumberRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadRecordViewModel();
            var user = ClaimsLoginUserData();
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;

            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadRecordModel> tripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
                // テーブル情報を変換
                tripRecordList = LoadRecordController.ConversionForTable(tripRecordList);

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
        /// テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<LoadRecordModel> records = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    var sql = NonIdentifyNumberRecordConnectController.CreateSQLToSelectNonIdentifyNumberRecords(startOfPeriod, endOfPeriod, checkedDepos);
                    records = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">便実績ID</th>
                                    <th class=""font-weight-bold"">到着実績</th>
                                    <th class=""font-weight-bold"">出発実績</th>
                                    <th class=""font-weight-bold"">稼働日</th>
                                    <th class=""font-weight-bold"">ステーション名</th>
                                    <th class=""font-weight-bold"">紐づけ切れ理由</th>
                                    <th class=""font-weight-bold"">到着荷量画像</th>
                                    <th class=""font-weight-bold"">出発荷量画像</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // テーブルのbody部分
                if (records.Count > 0)
                {
                    foreach (var record in records)
                    {
                        var arrivedAt = record.ArrivedAt.ToString("yyyy/MM/dd HH:mm");
                        if (arrivedAt == "0001/01/01 00:00")
                            arrivedAt = "-";
                        var departedAt = record.DepartedAt.ToString("yyyy/MM/dd HH:mm");
                        if (departedAt == "0001/01/01 00:00")
                            departedAt = "-";
                        var unlinkedReason = record.UnlinkedReasonName;
                        if (string.IsNullOrEmpty(unlinkedReason)) unlinkedReason = "-";
                        searchData += $@"
                            <tr>
                                <td>{record.TripRecordID}</td>
                                <td>{arrivedAt}</td>
                                <td>{departedAt}</td>
                                <td>{record.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>{record.StationName}</td>
                                <td>{unlinkedReason}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalNonIdentifyNumberLoadImageClick('{record.TripRecordID}', this, 'NonIdentifyNumberRecord')"" data-id=""{record.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnDepartureNonIdentifyNumberLoadImageClick('{record.TripRecordID}', this, 'NonIdentifyNumberRecord')"" data-id=""{record.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
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
                    searchedTripRecordLength = records.Count()
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

                var nonIdentifyNumberRecordDT = new DataTable();

                if (checkedDepos.Count > 0)
                {
                    var sql = NonIdentifyNumberRecordConnectController.CreateSQLToSelectNonIdentifyNumberRecordsForDataTable(startOfPeriod, endOfPeriod, checkedDepos);
                    nonIdentifyNumberRecordDT = ConnectToSQLServer.ConnectToDataTable(sql);
                }


                // 便実績が0の場合
                if (nonIdentifyNumberRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"紐づけ切れ実績_識別番号無_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "紐づけ切れ実績_識別番号無";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(nonIdentifyNumberRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, checkedDepos);

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
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download"></param>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
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
                    var sql = NonIdentifyNumberRecordConnectController.CreatSQLToSelectTripRecordForImage(startOfPeriod, endOfPeriod, checkedDepos);
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
                            // 便実績ID取得
                            var tripRecordID = tripRecord.TripRecordID;

                            // ファイルネームの指定
                            var FileNameArrive = string.Format($"{date}_A_{arrivalLoadStatus}_{tripRecordID}.jpg");
                            var FileNameDeparture = string.Format($"{date}_D_{departureLoadStatus}_{tripRecordID}.jpg");

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