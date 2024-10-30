using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using X.PagedList;
using static ai_truck_load_measurement.Models.TopModel;
using System.IO;
using System.Drawing;
using System;
using SixLabors.ImageSharp.Formats;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// トップ画面
    /// </summary>
    public class TopController : BaseController
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        public IActionResult Index()
        {
            // トップ画面モデル取得
            TopModel topModel = GetTopModel();
            return View(topModel);
        }

        /// <summary>
        /// トップ画面モデル取得
        /// </summary>
        /// <returns></returns>
        public TopModel GetTopModel()
        {
            TopModel topModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // 最新のステーション状況取得SQL作成
                var latestStationStatusSQL = TopConnectController.CreateSQLToSelectLatestStationStatus();
                // 最新のステーション状況取得
                List<TopModel> topModelList = TopConnectController.ConnectTops(latestStationStatusSQL, "AI-truck-load-measurement_test");
                // トラック有無取得SQL作成
                var isExistTrucksSQL = TopConnectController.CreateSQLToSelectIsExistTrucksPerStationID();
                // トラック有無取得
                IEnumerable<TopModel> isExistTrucksList = TopConnectController.ConnectTops(isExistTrucksSQL, "AI-truck-load-measurement_test");
                foreach (var item in topModelList)
                {
                    var isExistTruck = isExistTrucksList.Where(x => x.StationID == item.StationID).ToList();
                    if (isExistTruck.Count != 1)
                    {
                        ViewData["ErrorMessage"] = "E3004: " + ErrorMessagesResources.E3004;
                        throw new Exception();
                    }
                    item.TruckExist = isExistTruck[0].TruckExist;
                }
                // 取得値の変換
                topModelList = ConversionOfGetValues(topModelList);
                // ステーションの画像取得
                topModelList = GetStationImage(topModelList);
                topModel.TopModelList = topModelList;
                return topModel;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return topModel;
            }
        }

        /// <summary>
        /// 取得値の変換
        /// </summary>
        /// <param name="models">対象のトップ画面モデルリスト</param>
        /// <returns></returns>
        private List<TopModel> ConversionOfGetValues(List<TopModel> models)
        {
            foreach (var model in models)
            {
                // 荷量クラスと車両の存在有無により分岐
                var loadClass = model.LoadClass;
                var truckExist = model.TruckExist;
                var truckStatus = "　";

                if (loadClass == 2) truckStatus = "0%";
                if (loadClass >= 3)
                {
                    model.TruckExist = true;
                    int lowerLimit = (loadClass - 3) * 10 + 1;
                    int upperLimit = (loadClass - 2) * 10;
                    truckStatus = ($"{lowerLimit}-{upperLimit}%");
                }
                model.TruckStatus = truckStatus;
            }
            return models;
        }

        /// <summary>
        /// ステーションの画像取得
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private List<TopModel> GetStationImage(List<TopModel> models)
        {
            foreach (var model in models)
            {
                string imageBase64 = model.ImageBase64;
                // 不正な画像の場合はダミー画像を表示する
                if (!CanDecodeImageBase64(imageBase64))
                {
                    var rootPath = Directory.GetCurrentDirectory();
                    var imagePath = Path.Combine(rootPath, @"wwwroot\images\NoImage.png");
                    model.ImageBase64 = "data:image/jpeg;base64," + ImageToBase64(imagePath);
                }
            }
            return models;
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
        /// Base64でデコードできるか判定する
        /// </summary>
        /// <param name="imageBase64">Base64変換文字列</param>
        /// <returns></returns>        
        public bool CanDecodeImageBase64(string imageBase64)
        {   
            try
            {
                string base64String = imageBase64.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
