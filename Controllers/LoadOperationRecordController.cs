using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Data;
using X.PagedList;
using Aspose.Cells;
using System.IO.Compression;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadOperationRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadOperationRecordModel();
            var today = DateTime.Now;
            List<DateTime> dates = new();
            dates.Add(today);
            // ログインユーザーのメインデポ情報取得
            var mainDepo = GetMainDepo();
            List<string> depoList = new();
            depoList.Add(mainDepo.DepoID.ToString());
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectTripNameFromWorkDays(dates, depoList);
                // DB接続
                List<SelectListItem> tripNameList = LoadRecordConnectController.ConnectTTripRecordsForTripName(sql);

                model.TripNameList = tripNameList;

                // 便実績情報取得SQL作成
                var sql2 = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadRecordModel> tripRecordList = LoadRecordConnectController.ConnectTTripRecords(sql2);
                // テーブル情報を変換
                tripRecordList = LoadRecordController.ConversionForTable(tripRecordList);

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
        /// 指定した期間内に存在する便名称のリストを取得してセレクトリストアイテム化する
        /// </summary>
        /// <param name="workDays">指定した稼働日</param>
        /// <returns></returns>
        public List<SelectListItem> GetTripNameFromWorkDay(List<DateTime> workDays, List<string> checkedDepos)
        {
            List<SelectListItem> tripRecordList = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectTripNameFromWorkDays(workDays, checkedDepos);
                // DB接続
                tripRecordList = LoadRecordConnectController.ConnectTTripRecordsForTripName(sql);

                return tripRecordList;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripRecordList;
            }
        }



        /// <summary>
        /// 稼働日と便名称が一致する便実績を取得する
        /// </summary>
        /// <param name="workDay">稼働日</param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public List<LoadRecordModel> SearchTrips(DateTime workDay, string tripName)
        {
            List<LoadRecordModel> loadStatuses = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectLoadClassFromSearchConditions(tripName, workDay);
                // DB接続
                loadStatuses = LoadRecordController.CommonSearchTrips(sql);

                return loadStatuses;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return loadStatuses;
            }
        }


        /// <summary>
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="workDays">稼働日</param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public JsonResult SearchData(List<DateTime> workDays, string tripName)
        {
            try
            {
                // 指定し稼働日と便名称の便マスター情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreateSQLToSelectLoadClassFromSearchConditionsForTable(workDays, tripName);
                var searchedTripRecordListModel = LoadRecordController.SearchData(sql, "LoadOperationRecord");

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
        /// <param name="gamenName">画面名</param>
        /// <param name="workDays">稼働日</param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, List<DateTime> workDays, string tripName, List<string> checkedDepos)
        {
            string? errorMessage;
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
                var selectedWorkDays = SelectedWorkDays(workDays);
                searchConditionDT.Rows.Add("選択された稼働日", selectedWorkDays);
                searchConditionDT.Rows.Add("便名称", tripName);


                // 便実績情報取得
                var tTripRecordSql = LoadOperationRecordConnectController.CreateSQLToSelectTripRecordForDataTable(workDays, tripName);
                DataTable tTripRecordDT = LoadRecordConnectController.ConnectTTripRecordToDataTable(tTripRecordSql);

                // 荷量のクラスを数値化
                tTripRecordDT = LoadRecordController.GetConvertedLoadClassDataTable(tTripRecordDT);

                // ファイル名
                var tmpFilename = $"荷量実績_{tripName}.xlsx";
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
        /// 選択された稼働日を1行にまとめる
        /// </summary>
        /// <param name="workDays">指定された稼働日</param>
        /// <returns></returns>
        private string SelectedWorkDays(List<DateTime> workDays)
        {
            var selectedWorkDays = "";
            for (int i = 0; i < workDays.Count; i++)
            {
                if (i != 0)
                {
                    selectedWorkDays += ",";
                }
                selectedWorkDays += workDays[i].ToString("yyyyMMdd");
            }
            return selectedWorkDays;
        }

        /// <summary>
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download">判定用</param>
        /// <param name="workDays">稼働日</param>
        /// <param name="selectedTripName">選択された便名称</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, List<DateTime> workDays, string selectedTripName, List<string> checkedDepos)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                // 指定した期間の便実績情報取得SQL作成
                var sql = LoadOperationRecordConnectController.CreatSQLToSelectTripRecordForImage(workDays, selectedTripName);
                // DB接続
                IEnumerable<LoadRecordModel> tripRecordList = LoadRecordConnectController.ConnectTTripRecords(sql);

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
                            var selectedWorkDays = SelectedWorkDays(workDays);
                            var selectedDepos = LoadRecordController.SelectedDepos(checkedDepos);
                            sw.WriteLine($"対象デポ：{selectedDepos}");
                            sw.WriteLine($"選択された稼働日:{selectedWorkDays}");
                            sw.WriteLine($"便名称：{selectedTripName}");
                        }
                    }


                    // メモリストリームを配列に変換してViewに渡す
                    return Json(new { data = File(ms.ToArray(), "application/zip", $"荷量画像_{selectedTripName}") });
                }
            }
            // エラーメッセージ取得
            // 「ファイルが存在しません。」
            var errorMessage = ErrorMessagesResources.E9999;

            return Json(new { res = "NG", error = errorMessage });
        }
    }
}
