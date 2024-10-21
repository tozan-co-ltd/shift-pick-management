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
            LoadOutputModel model = new();
            // 初期表示の日付を取得
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);
            // 荷量の相違なしも表示
            var isOnlyHasAmountDefference = false;

            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecord(oneWeekAgo, today, isOnlyHasAmountDefference);
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList =LoadOutputConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");
                // テーブル情報を変換
                tripRecordList = ConversionForTable(tripRecordList);

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
        /// テーブル情報を変換
        /// </summary>
        /// <param name="models">変換元</param>
        /// <returns></returns>
        private IEnumerable<LoadOutputModel> ConversionForTable(IEnumerable<LoadOutputModel> models)
        {
            foreach (var model in models)
            {
                var arrivalLoadClass = model.ArrivalLoadClass;
                var departureLoadClass = model.DepartureLoadClass;

                // 到着荷量クラスと出発荷量クラスをそれぞれ変換
                model.ArrivalLoadStatus = ConversionLoadClassToLoadStatus(arrivalLoadClass);
                model.DepartureLoadStatus = ConversionLoadClassToLoadStatus(departureLoadClass);

                // テーブルの空欄を"-"に変換
                if (string.IsNullOrEmpty(model.TripName)) model.TripName = "-";
                if (string.IsNullOrEmpty(model.TripBranchSeq)) model.TripBranchSeq = "-";
                if (string.IsNullOrEmpty(model.DriverName)) model.DriverName = "-";

                model.IdentifyNumber = ConvertNumberToFourDigitOrHyphen(model.IdentifyNumber);
            }
            return models;
        }

        /// <summary>
        /// 荷量クラスからパーセント表示に変換
        /// </summary>
        /// <param name="loadClass">荷量クラス</param>
        /// <returns></returns>
        private string ConversionLoadClassToLoadStatus(int loadClass)
        {
            var loadStatus = "-";
            if (loadClass >= 3)
            {
                int lowerLimit = (loadClass - 3) * 10 + 1;
                int upperLimit = (loadClass - 2) * 10;
                loadStatus = ($"{lowerLimit}-{upperLimit}");
            }
            return loadStatus;
        }

        /// <summary>
        /// 画像のパスが正しいかどうかのチェックとパスの変換
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private string CheckAndConvertImagePath(string imagePath)
        {
            // 画像パスに画像がないかパスが不正な場合はダミー画像を表示する
            if (!IsValidImage(imagePath))
            {
                var rootPath = Directory.GetCurrentDirectory();
                imagePath = Path.Combine(rootPath, @"wwwroot\images\NoImage.png");
            }
            var imagePathToBase64 = ImageToBase64(imagePath);
            return imagePathToBase64;
        }

        /// <summary>
        /// 画像のパスをBase64文字列に変換する
        /// </summary>
        /// <param name="imagePath">変換したい画像のパス</param>
        /// <returns></returns>
        private static string ImageToBase64(string imagePath)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Jpeg); // 画像フォーマットを指定（ここではJPEG）
                    byte[] imageBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }


        /// <summary>
        /// 画像のパスが正しいかどうか確認する
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>        
        public bool IsValidImage(string imagePath)
        {
            // 画像パスがここに含まれたフォーマットの場合trueを返す
            var imageFormats = new List<ImageFormat>()
                  {
                    ImageFormat.Jpeg,
                    ImageFormat.Png,
                  };
            try
            {

                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                using (Image targetImage = Image.FromStream(fileStream))
                {
                    return imageFormats.Contains(targetImage.RawFormat);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference)
        {
            var searchData = string.Empty;
            IEnumerable<LoadOutputModel> tripRecordList;
            try
            {
                // 便マスター情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecord(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference);
                // DB接続
                tripRecordList = LoadOutputConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");
                // 荷量のクラスを数値に、画像パスをBase64に変換
                tripRecordList = ConversionForTable(tripRecordList);
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th hidden>便実績ID</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">便枝番</th>
                                    <th class=""font-weight-bold"">乗務員</th>
                                    <th class=""font-weight-bold"">ステーションID</th>
                                    <th class=""font-weight-bold"">車両番号</th>
                                    <th class=""font-weight-bold"">識別番号</th>
                                    <th class=""font-weight-bold"">到着予定時間</th>
                                    <th class=""font-weight-bold"">出発予定時間</th>
                                    <th class=""font-weight-bold"">稼働日</th>
                                    <th class=""font-weight-bold"">到着日時</th>
                                    <th class=""font-weight-bold"">出発日時</th>
                                    <th class=""font-weight-bold"">到着荷量(%)</th>
                                    <th class=""font-weight-bold"">出発荷量(%)</th>
                                    <th class=""font-weight-bold"">到着荷量画像</th>
                                    <th class=""font-weight-bold"">出発荷量画像</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // 新しい便情報テーブルのhtml作成
                if (tripRecordList.Count() > 0)
                {
                    foreach (var item in tripRecordList)
                    {
                        var truckNumber = item.TruckNumber.ToString();
                        if (truckNumber == "0") truckNumber = "-";
                        var arrivalScheduledTime = item.ArrivalScheduledTime.ToString("HH:mm");
                        if (arrivalScheduledTime == "00:00") arrivalScheduledTime = "-";
                        var departureScheduledTime = item.DepartureScheduledTime.ToString("HH:mm");
                        if(departureScheduledTime == "00:00") departureScheduledTime = "-";
                        searchData += $@"
                            <tr>
                                <td hidden>{item.TripRecordID}</td>
                                <td>{item.TripName}</td>
                                <td>{item.TripBranchSeq}</td>
                                <td>{item.DriverName}</td>
                                <td>{item.StationID}</td>
                                <td>{truckNumber}</td>
                                <td>{item.IdentifyNumber}</td>
                                <td>{arrivalScheduledTime}</td>
                                <td>{departureScheduledTime}</td>
                                <td>{item.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>{item.ArrivedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{item.DepartedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{item.ArrivalLoadStatus}</td>
                                <td>{item.DepartureLoadStatus}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnArrivalLoadImageClick('{item.TripRecordID}', this)"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnDepartureLoadImageClick('{item.TripRecordID}', this)"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
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

                var searchedTripRecordListModel = new SearchedTripRecordListModel()
                {
                    searchedTripRecordHTML = searchData,
                    searchedTripRecordLength = tripRecordList.Count()
                };

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
        /// 数値を4桁表示またはハイフンに変更する
        /// </summary>
        /// <param name="number">変更したい数値</param>
        /// <returns></returns>
        private string ConvertNumberToFourDigitOrHyphen(string? number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return "-";
            }
            number = number.PadLeft(4, '0');
            if (number == "0000") number = "-";
            return number;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference)
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
                searchConditionDT.Rows.Add("期間",$"{startDate}~{endDate}");

                // 便実績情報取得
                var tTripRecordSql = LoadOutputConnectController.CreateSQLToSelectTripRecordForDataTable(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference);
                DataTable tTripRecordDT = LoadOutputConnectController.ConnectTTripRecordToDataTable(tTripRecordSql, "AI-truck-load-measurement_test");

                // 荷量のクラスを数値化
                tTripRecordDT = GetConvertedLoadClassDataTable(tTripRecordDT);

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
        /// データテーブルの荷量クラスを数値に変換
        /// </summary>
        /// <param name="dt">変換元データテーブル</param>
        /// <returns></returns>
        private DataTable GetConvertedLoadClassDataTable(DataTable dt)
        {
            // テーブルに値を変換した後の文字列を格納する列を追加
            dt.Columns.Add("converted_branch_seq", typeof(string)).SetOrdinal(1);
            dt.Columns.Add("converted_truck_number", typeof(string)).SetOrdinal(5);
            dt.Columns.Add("converted_identify_number", typeof(string)).SetOrdinal(6);
            dt.Columns.Add("converted_arrival_scheduled_time", typeof(string)).SetOrdinal(7);
            dt.Columns.Add("converted_departure_scheduled_time", typeof(string)).SetOrdinal(8);
            dt.Columns.Add("arrival_load_status", typeof(string)).SetOrdinal(16);
            dt.Columns.Add("departure_load_status", typeof(string)).SetOrdinal(17);

            // 各列の値を適切な値に変換
            foreach (DataRow row in dt.Rows)
            {
                // 荷量クラスを%表示に変換
                var arrivalLoadClass = (int)row["arrival_load_class"];
                var departureLoadClass = (int)row["departure_load_class"];
                row["arrival_load_status"] = ConversionLoadClassToLoadStatus(arrivalLoadClass);
                row["departure_load_status"] = ConversionLoadClassToLoadStatus(departureLoadClass);

                // 各列の値が空白の場合、"-"に変換する
                if (string.IsNullOrEmpty(row["trip_name"].ToString())) row["trip_name"] = "-";
                if (string.IsNullOrEmpty(row["driver_name"].ToString())) row["driver_name"] = "-";
                row["converted_identify_number"] = ConvertNumberToFourDigitOrHyphen(row["identify_number"].ToString());
                ConvertString(row, "trip_branch_seq", "converted_branch_seq");
                ConvertString(row, "truck_number", "converted_truck_number");
                ConvertString(row, "arrival_scheduled_time", "converted_arrival_scheduled_time");
                ConvertString(row, "departure_scheduled_time", "converted_departure_scheduled_time");
            }

            // 変換前の列を削除
            dt.Columns.Remove("trip_branch_seq");
            dt.Columns.Remove("truck_number");
            dt.Columns.Remove("identify_number");
            dt.Columns.Remove("arrival_scheduled_time");
            dt.Columns.Remove("departure_scheduled_time");
            dt.Columns.Remove("arrival_load_class");
            dt.Columns.Remove("departure_load_class");
            return dt;
        }

        /// <summary>
        /// 列の値を文字列に変換して違う列に格納する
        /// </summary>
        /// <param name="row">行データ</param>
        /// <param name="beforeColumnName">変換したい列名</param>
        /// <param name="afterColumnName">変換後の列名</param>
        private void ConvertString(DataRow row, string beforeColumnName, string afterColumnName)
        {
            if (string.IsNullOrEmpty(row[beforeColumnName].ToString()))
            {
                row[afterColumnName] = "-";
            }
            else
            {
                row[afterColumnName] = row[beforeColumnName].ToString();
            }
        }

        /// <summary>
        /// 荷量の相違ありテーブルの設定値を保存する
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public IActionResult RegistOrInsertAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 「荷量の相違あり」で保存した値が既に存在するか
                var isSameAnnotationLoadsExist = LoadOutputConnectController.IsSameAnnotationLoadsExist(tripRecordID, isArrived);

                // 「荷量の相違あり」の設定値を更新、保存
                if (isSameAnnotationLoadsExist)
                {
                    // 更新
                    LoadOutputConnectController.UpdateAnnotationLoads(tripRecordID, loadStatus, isArrived, user, "AI-truck-load-measurement_test");
                }
                else
                {
                    // 新規保存
                    LoadOutputConnectController.InsertAnnotationLoads(tripRecordID, loadStatus, isArrived, user, "AI-truck-load-measurement_test");
                }

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
        /// 荷量画像モーダルに表示する値の取得
        /// </summary>
        /// <param name="model">モーダルに表示するモデル</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public LoadOutputModel GetModalItems(LoadOutputModel model, bool isArrived)
        {
            // 「荷量の相違あり」で保存した値が既に存在するか
            var isSameAnnotationLoadsExist = LoadOutputConnectController.IsSameAnnotationLoadsExist(model.TripRecordID, isArrived);
            if (isSameAnnotationLoadsExist)
            {
                var annotationLoadClass = LoadOutputConnectController.GetAnnotationLoadClassByTripRecordIDAndIsArrived(model.TripRecordID, isArrived, "AI-truck-load-measurement_test");
                model.AnnotationLoadClass = annotationLoadClass;
            }

            // ステーションの画像取得
            model.ArrivalLoadImgPath = CheckAndConvertImagePath(model.ArrivalLoadImgPath);
            model.DepartureLoadImgPath = CheckAndConvertImagePath(model.DepartureLoadImgPath);
            
            return model;
        }

        /// <summary>
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download"></param>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみのデータか</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                // 便実績情報取得SQL作成
                var sql = LoadOutputConnectController.CreatSQLToSelectTripRecord(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference);
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList = LoadOutputConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

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
                            var arrivalLoadImgPath = CheckAndConvertImagePath(tripRecord.ArrivalLoadImgPath);
                            var departureLoadImgPath = CheckAndConvertImagePath(tripRecord.DepartureLoadImgPath);
                            byte[] arrivalLoadImgBytes = Convert.FromBase64String(arrivalLoadImgPath);
                            byte[] departureLoadImgBytes = Convert.FromBase64String(departureLoadImgPath);
                            // 荷量取得
                            var arrivalLoadStatus = ConversionLoadClassToLoadStatus(tripRecord.ArrivalLoadClass);
                            var departureLoadStatus = ConversionLoadClassToLoadStatus(tripRecord.DepartureLoadClass);
                            date = tripRecord.WorkDay.ToString("yyyyMMdd");

                        
                                // ファイルネームの指定
                            var FileNameArrive = string.Format($"{tripRecord.TripName}_{tripRecord.TripBranchSeq}_{date}_A_{arrivalLoadStatus}.jpg");
                            var FileNameDeparture = string.Format($"{tripRecord.TripName}_{tripRecord.TripBranchSeq}_{date}_D_{departureLoadStatus}.jpg");

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
                            sw.Write($"期間：{startDate}~{endDate}");
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

