using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using X.PagedList;
using System.Drawing;
using ai_truck_load_measurement.Commons;
using System.Data;
using System.IO.Compression;
using System.Collections.Generic;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            var model = new LoadTransitionModel();
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);
            var mainDepo = GetMainDepo();
            List<string> depoList = new();
            depoList.Add(mainDepo.DepoID.ToString());
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadRecordConnectController.CreateSQLToSelectTripNameFromPeriod(oneWeekAgo, today, depoList);
                // DB接続
                List<SelectListItem> tripNameList = LoadRecordConnectController.ConnectTTripRecordsForTripName(sql);

                model.TripNameList = tripNameList;

                // 便実績情報取得SQL作成
                var sql2 = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadTransitionModel> tripRecordList = LoadTransitionConnectController.ConnectTTripRecords(sql2);
                // テーブル情報を変換
                tripRecordList = (IEnumerable<LoadTransitionModel>)LoadRecordController.ConversionForTable(tripRecordList);

                model.TripRecordList = tripRecordList.ToPagedList();

                model.MainDepoID = mainDepo.DepoID;
                model.MainDepoName = mainDepo.Name;
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
        /// 期間内で便名称と便枝番が一致する便実績データのリストを取得する
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <param name="tripBranchSeq">便枝番</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public List<LoadRecordModel> SearchTrips(string tripName, int tripBranchSeq, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectLoadClassFromSearchConditions(tripName, tripBranchSeq, startOfPeriod, endOfPeriod);
                // DB接続
                var loadStatuses = LoadRecordController.CommonSearchTrips(sql);

                return loadStatuses;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult SearchData(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            try
            {
                // 指定した期間の便マスター情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectLoadClassFromSearchConditionsForTable(models, startOfPeriod, endOfPeriod);
                var searchedTripRecordListModel = LoadRecordController.SearchData(sql, "LoadTransition");

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

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, List<LoadRecordModel> arrayTrips, List<string> checkedDepos)
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
                var selectedTripNames = "";
                for(int i=0; i<arrayTrips.Count; i++)
                {
                    if(i != 0)
                    {
                        selectedTripNames += ", ";
                    }
                    selectedTripNames += arrayTrips[i].SelectedTripName;
                }
                searchConditionDT.Rows.Add("選択された便", selectedTripNames);

                // 便実績情報取得
                var tTripRecordSql = LoadTransitionConnectController.CreateSQLToSelectTripRecordForDataTable(arrayTrips, startOfPeriod, endOfPeriod);
                DataTable tTripRecordDT = LoadRecordConnectController.ConnectTTripRecordToDataTable(tTripRecordSql);

                // 荷量のクラスを数値化
                tTripRecordDT = LoadRecordController.GetConvertedLoadClassDataTable(tTripRecordDT);

                // ファイル名
                var tmpFilename = $"荷量実績_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "検索条件シート";
                string sheetNameTwo = "荷量実績シート";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(searchConditionDT, tTripRecordDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);


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
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみのデータか</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod, List<LoadRecordModel> arrayTrips, List<string> checkedDepos)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                // 指定した期間の便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreatSQLToSelectTripRecordForImage(arrayTrips, startOfPeriod, endOfPeriod);
                // DB接続
                IEnumerable<LoadTransitionModel> tripRecordList = LoadTransitionConnectController.ConnectTTripRecords(sql);

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
                            var selectedDepos = LoadRecordController.SelectedDepos(checkedDepos);
                            sw.WriteLine($"対象デポ：{selectedDepos}");
                            sw.WriteLine($"稼働日：{startDate}～{endDate}");
                            // 選択された便の羅列
                            var selectedTripNames = "";
                            for (int i = 0; i < arrayTrips.Count; i++)
                            {
                                if (i != 0)
                                {
                                    selectedTripNames += ", ";
                                }
                                selectedTripNames += arrayTrips[i].SelectedTripName;
                            }
                            sw.WriteLine($"選択された便：{selectedTripNames}");
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

