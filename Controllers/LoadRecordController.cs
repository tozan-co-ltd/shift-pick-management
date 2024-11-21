using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using System.Data;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Properties;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 各実績画面の共通処理用コントローラー
    /// </summary>
    public class LoadRecordController : BaseController
    {

        /// <summary>
        /// テーブル情報を変換
        /// </summary>
        /// <param name="models">変換元</param>
        /// <returns></returns>
        public static IEnumerable<LoadRecordModel> ConversionForTable(IEnumerable<LoadRecordModel> models)
        {
            foreach (var model in models)
            {
                var arrivalLoadClass = model.ArrivalLoadClass;
                var departureLoadClass = model.DepartureLoadClass;

                // 到着荷量クラスと出発荷量クラスをそれぞれ変換
                model.ArrivalLoadStatus =　ConversionLoadClassToLoadStatus(arrivalLoadClass);
                model.DepartureLoadStatus = ConversionLoadClassToLoadStatus(departureLoadClass);

                // テーブルの空欄を"-"に変換
                if (string.IsNullOrEmpty(model.TruckNumber)) model.TruckNumber = "-";
                if (string.IsNullOrEmpty(model.TripName)) model.TripName = "-";
                if (string.IsNullOrEmpty(model.TripBranchSeq)) model.TripBranchSeq = "-";
                if (string.IsNullOrEmpty(model.DriverName)) model.DriverName = "-";

                model.IdentifyNumber = ConvertNumberToFourDigitOrHyphen(model.IdentifyNumber);
            }
            return models;
        }

        /// <summary>
        /// データテーブルの荷量クラスを数値に変換
        /// </summary>
        /// <param name="dt">変換元データテーブル</param>
        /// <returns></returns>
        public static DataTable GetConvertedLoadClassDataTable(DataTable dt)
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
        /// 数値を4桁表示またはハイフンに変更する
        /// </summary>
        /// <param name="number">変更したい数値</param>
        /// <returns></returns>
        public static string ConvertNumberToFourDigitOrHyphen(string? number)
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
        public static string ConversionLoadClassToLoadStatus(int loadClass)
        {
            var loadStatus = "-";
            if (loadClass >= 3)
            {
                int lowerLimit = (loadClass - 3) * 10 + 1;
                int upperLimit = (loadClass - 2) * 10;
                loadStatus = ($"{lowerLimit}-{upperLimit}");
            }
            else if (loadClass == 2)
            {
                loadStatus = "0";
            }
            return loadStatus;
        }

        /// <summary>
        /// 荷量クラスからチャート用のパーセント表示に変換
        /// </summary>
        /// <param name="loadClass">荷量クラス</param>
        /// <returns></returns>
        public static string ConversionLoadClassToLoadStatusForChart(int loadClass)
        {
            var loadStatus = "";
            if (loadClass >= 3)
            {
                loadStatus = ((loadClass - 3) * 10 + 5).ToString();
            }
            else if (loadClass == 2)
            {
                loadStatus = "0";
            }
            return loadStatus;
        }

        /// <summary>
        /// 列の値を文字列に変換して違う列に格納する
        /// </summary>
        /// <param name="row">行データ</param>
        /// <param name="beforeColumnName">変換したい列名</param>
        /// <param name="afterColumnName">変換後の列名</param>
        private static void ConvertString(DataRow row, string beforeColumnName, string afterColumnName)
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
        /// 荷量画像モーダルに表示する値の取得
        /// </summary>
        /// <param name="model">モーダルに表示するモデル</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public LoadRecordModel GetModalItems(LoadRecordModel model, bool isArrived)
        {
            var isSameAnnotationLoadsExist = LoadRecordConnectController.IsSameAnnotationLoadsExist(model.TripRecordID, isArrived);
            if (isSameAnnotationLoadsExist)
            {
                var annotationLoadClass = LoadRecordConnectController.GetAnnotationLoadClassByTripRecordIDAndIsArrived(model.TripRecordID, isArrived);
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
        public static string CheckAndConvertImagePath(string imagePath)
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


        // <summary>
        /// 画像のパスが正しいかどうか確認する
        /// </summary>
        /// <param name="imagePath">確認したい画像パス</param>
        /// <returns></returns>        
        public static bool IsValidImage(string imagePath)
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
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static SearchedTripRecordListModel SearchData(string sql)
        {
            var searchData = string.Empty;
            IEnumerable<LoadRecordModel> tripRecordList;
            // DB接続
            tripRecordList = LoadRecordConnectController.ConnectTTripRecords(sql);
            // 荷量のクラスを数値に、画像パスをBase64に変換
            tripRecordList = ConversionForTable(tripRecordList);
            searchData += $@"
                <div class=""mt-3"">
                    <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                        <thead>
                            <tr align=""center"">
                                <th hidden>便実績ID</th>
                                <th class=""font-weight-bold"">便名称<br></th>
                                <th class=""font-weight-bold"">便枝番</th>
                                <th class=""font-weight-bold"">乗務員</th>
                                <th class=""font-weight-bold"">ステーション<br>ID</th>
                                <th class=""font-weight-bold"">車両<br>番号</th>
                                <th class=""font-weight-bold"">識別<br>番号</th>
                                <th class=""font-weight-bold"">到着<br>予定</th>
                                <th class=""font-weight-bold"">出発<br>予定</th>
                                <th class=""font-weight-bold"">稼働日</th>
                                <th class=""font-weight-bold"">到着日時</th>
                                <th class=""font-weight-bold"">出発日時</th>
                                <th class=""font-weight-bold"">到着荷量<br>(%)</th>
                                <th class=""font-weight-bold"">出発荷量<br>(%)</th>
                                <th class=""font-weight-bold"">到着荷量<br>画像</th>
                                <th class=""font-weight-bold"">出発荷量<br>画像</th>
                            </tr>
                        </thead>
                        <tbody>
            ";
            // 新しい便情報テーブルのhtml作成
            if (tripRecordList.Count() > 0)
            {
                foreach (var item in tripRecordList)
                {
                    var truckNumber = item.TruckNumber;
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

            return searchedTripRecordListModel;
        }

        /// <summary>
        /// 荷量の相違ありテーブルの設定値を保存する
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public IActionResult InsertOrUpdateAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived)
        {
            string? errorMessage;
            NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                // 初期値でクリックした場合は何も起こらない
                if (loadStatus == 0)
                {
                    return NotFound();
                }

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 「荷量の相違あり」で保存した値が既に存在するか
                var isSameAnnotationLoadsExist = LoadRecordConnectController.IsSameAnnotationLoadsExist(tripRecordID, isArrived);

                // 「荷量の相違あり」の設定値を更新、保存
                if (isSameAnnotationLoadsExist)
                {
                    // 更新
                    LoadRecordConnectController.UpdateAnnotationLoads(tripRecordID, loadStatus, isArrived, user);
                }
                else
                {
                    // 新規保存
                    LoadRecordConnectController.InsertAnnotationLoads(tripRecordID, loadStatus, isArrived, user);
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
                var sql = LoadRecordConnectController.CreateSQLToSelectTripNameFromPeriod(startOfPeriod, endOfPeriod);
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
                var sql = LoadRecordConnectController.CreateSQLToSelectTripBranchSeqFromTripName(tripName, startOfPeriod, endOfPeriod);
                // DB接続
                tripBranchSeqList = LoadRecordConnectController.ConnectTTripRecordsForTripBranchSeq(sql);

                return tripBranchSeqList;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripBranchSeqList;
            }
        }
    }
}
