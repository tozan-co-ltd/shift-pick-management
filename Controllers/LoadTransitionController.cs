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

namespace ai_truck_load_measurement.Controllers
{
    public class LoadTransitionController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadTransitionModel();
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectTripNameFromPeriod(oneWeekAgo, today);
                // DB接続
                List<SelectListItem> tripNameList = LoadTransitionConnectController.ConnectTTripRecordsForTripName(sql, "AI-truck-load-measurement_test");

                model.TripNameList = tripNameList;

                // 便実績情報取得SQL作成
                var sql2 = LoadTransitionConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadTransitionModel> tripRecordList = LoadTransitionConnectController.ConnectTTripRecords(sql2, "AI-truck-load-measurement_test");
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
        /// 指定した期間内に存在する便名称のリストを取得してセレクトリストアイテム化する
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public List<SelectListItem> GetTripNameFromPeriod(DateTime startOfPeriod, DateTime endOfPeriod)
        {
            List<SelectListItem> tripRecordList = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectTripNameFromPeriod(startOfPeriod, endOfPeriod);
                // DB接続
                tripRecordList = LoadTransitionConnectController.ConnectTTripRecordsForTripName(sql, "AI-truck-load-measurement_test");

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
        /// 便名称から指定した期間内の便枝番のリストを取得する
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public List<int> GetTripBranchSeqFromTripName(string tripName, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            List<int> tripBranchSeqList = new();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectTripBranchSeqFromTripName(tripName, startOfPeriod, endOfPeriod);
                // DB接続
                tripBranchSeqList = LoadTransitionConnectController.ConnectTTripRecordsForTripBranchSeq(sql, "AI-truck-load-measurement_test");

                return tripBranchSeqList;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripBranchSeqList;
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
        public List<RequestLoadStatus> SearchTrips(string tripName, int tripBranchSeq, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            List<RequestLoadStatus> loadStatuses = new ();
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreateSQLToSelectLoadClassFromSearchConditions(tripName, tripBranchSeq, startOfPeriod, endOfPeriod);
                // DB接続
                var loadClasses = LoadTransitionConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

                foreach ( var loadClass in loadClasses)
                {
                    var loadStatus = new RequestLoadStatus
                    {
                        WorkDay = loadClass.WorkDay,
                        ArrivalLoadStatus = ConversionLoadClassToLoadStatusForChart(loadClass.ArrivalLoadClass),
                        DepartureLoadStatus = ConversionLoadClassToLoadStatusForChart(loadClass.DepartureLoadClass)
                    };
                    loadStatuses.Add(loadStatus);
                }
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
        /// 荷量クラスからチャート用のパーセント表示に変換
        /// </summary>
        /// <param name="loadClass">荷量クラス</param>
        /// <returns></returns>
        private string ConversionLoadClassToLoadStatusForChart(int loadClass)
        {
            var loadStatus = "";
            if (loadClass >= 3)
            { 
                loadStatus = ((loadClass - 3) * 10 + 5).ToString() ;
            }
            return loadStatus;
        }

        /// <summary>
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var searchData = string.Empty;
            IEnumerable<LoadTransitionModel> tripRecordList;
            try
            {
                // 指定した期間の便マスター情報取得SQL作成
                var sql = LoadTransitionConnectController.CreatSQLToSelectTripRecordFromPeriod(startOfPeriod, endOfPeriod);
                // DB接続
                tripRecordList = LoadTransitionConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");
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
                        if (departureScheduledTime == "00:00") departureScheduledTime = "-";
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
        /// テーブル情報を変換
        /// </summary>
        /// <param name="models">変換元</param>
        /// <returns></returns>
        private IEnumerable<LoadTransitionModel> ConversionForTable(IEnumerable<LoadTransitionModel> models)
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
        /// 荷量画像モーダルに表示する値の取得
        /// </summary>
        /// <param name="model">モーダルに表示するモデル</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public LoadTransitionModel GetModalItems(LoadTransitionModel model, bool isArrived)
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
        /// 画像のパスが正しいかどうかのチェックとパスの変換
        /// </summary>
        /// <param name="imagePath">画像パス</param>
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

        // <summary>
        /// 画像のパスが正しいかどうか確認する
        /// </summary>
        /// <param name="imagePath">確認したい画像パス</param>
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
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
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
                searchConditionDT.Rows.Add("期間", $"{startDate}～{endDate}");

                // 便実績情報取得
                var tTripRecordSql = LoadTransitionConnectController.CreateSQLToSelectTripRecordForDataTable(startOfPeriod, endOfPeriod);
                DataTable tTripRecordDT = LoadTransitionConnectController.ConnectTTripRecordToDataTable(tTripRecordSql, "AI-truck-load-measurement_test");

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
        /// 画像一括ダウンロード
        /// </summary>
        /// <param name="download"></param>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみのデータか</param>
        /// <returns></returns>
        public JsonResult ZipDownload(string download, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            // ダウンロードボタンが押された際の処理
            if (download == "download")
            {
                // 指定した期間の便実績情報取得SQL作成
                var sql = LoadTransitionConnectController.CreatSQLToSelectTripRecordForImage(startOfPeriod, endOfPeriod);
                // DB接続
                IEnumerable<LoadTransitionModel> tripRecordList = LoadTransitionConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

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
                            sw.Write($"期間：{startDate}～{endDate}");
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

