using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using X.PagedList;
using System.Drawing;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Controllers
{
    public class T_TripRecordController : BaseController
    {
        public IActionResult Index()
        {
            T_TripRecordModel model = new();
            // 初期表示の日付を取得
            var today = DateTime.Now;
            var oneWeekAgo = today.AddDays(-7);

            try
            {
                // 便実績情報取得SQL作成
                var sql = T_TripRecordConnectController.CreatSQLToSelectTripRecord(oneWeekAgo, today);
                // DB接続
                IEnumerable<T_TripRecordModel> tripRecordList =T_TripRecordConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

                tripRecordList = ConversionOfGetValues(tripRecordList);

                tripRecordList = GetStationImage(tripRecordList);

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
        /// 取得値の変換
        /// </summary>
        /// <param name="models">対象のトップ画面モデルリスト</param>
        /// <returns></returns>
        private IEnumerable<T_TripRecordModel> ConversionOfGetValues(IEnumerable<T_TripRecordModel> models)
        {
            foreach (var model in models)
            {
                var arrivalLoadClass = model.ArrivalLoadClass;
                var departureLoadClass = model.DepartureLoadClass;

                // 到着荷量クラスと出発荷量クラスをそれぞれ変換
                model.ArrivalLoadStatus = ConversionLoadClassToLoadStatus(arrivalLoadClass);
                model.DepartureLoadStatus = ConversionLoadClassToLoadStatus(departureLoadClass);
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
            var loadStatus = string.Empty;
            if (loadClass < 3)
            {
                loadStatus = "　";
            }
            else
            {
                int lowerLimit = (loadClass - 3) * 10 + 1;
                int upperLimit = (loadClass - 2) * 10;
                loadStatus = ($"{lowerLimit}~{upperLimit}%");
            }
            return loadStatus;
        }

        /// <summary>
        /// ステーションの画像取得
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private IEnumerable<T_TripRecordModel> GetStationImage(IEnumerable<T_TripRecordModel> models)
        {
            foreach (var model in models)
            {
                model.ArrivalLoadImgPath = CheckAndConvertImagePath(model.ArrivalLoadImgPath);
                model.DepartureLoadImgPath = CheckAndConvertImagePath(model.DepartureLoadImgPath);
            }
            return models;
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
        public IActionResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var searchData = string.Empty;
            List<T_TripRecordModel> tripRecordList = new();
            try
            {
                // 便マスター情報取得SQL作成
                var sql = T_TripRecordConnectController.CreatSQLToSelectTripRecord(startOfPeriod, endOfPeriod);
                // DB接続
                tripRecordList = T_TripRecordConnectController.ConnectTTripRecords(sql, "AI-truck-load-measurement_test");

                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">便枝番</th>
                                    <th class=""font-weight-bold"">乗務員</th>
                                    <th class=""font-weight-bold"">ステーションID</th>
                                    <th class=""font-weight-bold"">車両ID</th>
                                    <th class=""font-weight-bold"">識別番号</th>
                                    <th class=""font-weight-bold"">到着予定時間</th>
                                    <th class=""font-weight-bold"">出発予定時間</th>
                                    <th class=""font-weight-bold"">作業日</th>
                                    <th class=""font-weight-bold"">到着日時</th>
                                    <th class=""font-weight-bold"">出発日時</th>
                                    <th class=""font-weight-bold"">到着荷量</th>
                                    <th class=""font-weight-bold"">出発荷量</th>
                                    <th class=""font-weight-bold"">到着荷量画像</th>
                                    <th class=""font-weight-bold"">出発荷量画像</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // 新しい便情報テーブルのhtml作成
                if (tripRecordList.Count > 0)
                {
                    foreach (var item in tripRecordList)
                    {
                        searchData += $@"
                            <tr>
                                <td>{item.TripName}</td>
                                <td>{item.TripBranchSeq}</td>
                                <td>{item.DriverName}</td>
                                <td>{item.StationID}</td>
                                <td>{item.TruckNumber}</td>
                                <td>{item.IdentifyNumber}</td>
                                <td>{item.ArrivalScheduledTime.ToString("HH:mm")}</td>
                                <td>{item.DepartureScheduledTime.ToString("HH:mm")}</td>
                                <td>{item.WorkDay.ToString("yyyy/MM/dd")}</td>
                                <td>{item.ArrivedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{item.DepartedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{item.ArrivalLoadStatus}</td>
                                <td>{item.DepartureLoadStatus}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnArrivalLoadImageClick('{item.TripRecordID}')"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnDepartureLoadImageClick('{item.TripRecordID}')"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
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

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Content(errorMessage);
            }
        }
    }
}

