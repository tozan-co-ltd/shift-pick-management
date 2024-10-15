using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using X.PagedList;
using System.Drawing;

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
    }
}

