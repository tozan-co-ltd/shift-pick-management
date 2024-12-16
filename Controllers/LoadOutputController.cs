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
            LoadOutputModel model = new();

            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();
            model.UserName = user.UserName;
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList =LoadOutputConnectController.ConnectTTripRecords(sql);
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
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber)
        {
            var searchData = string.Empty;
            IEnumerable<LoadOutputModel> tripRecordList;
            try
            {
                // 指定した期間の便マスター情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecordFromPeriod(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
                var searchedTripRecordListModel = LoadRecordController.SearchData(sql, "LoadOutput");

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
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber)
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
                searchConditionDT.Rows.Add("稼働日",$"{startDate}～{endDate}");
                // 絞り込み条件作成
                var shiborikomiCondition = ShiborikomiCondition(isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
                searchConditionDT.Rows.Add("絞り込み条件：",shiborikomiCondition);

                // 便実績情報取得
                var tTripRecordSql = LoadOutputConnectController.CreateSQLToSelectTripRecordForDataTable(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
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
                shiborikomiCondition = "荷量の相違ありのみ";
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
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                // 指定した期間の便実績情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecordForImage(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber);
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList = LoadOutputConnectController.ConnectTTripRecords(sql);

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

