using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using X.PagedList;
using static ai_truck_load_measurement.Models.ViewCardModel;
using System.IO;
using System.Drawing;
using System;
using SixLabors.ImageSharp.Formats;
using System.Collections;

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
        public async Task<IActionResult> Index()
        {
            // ログインユーザーのメインデポ情報取得
            var user = ClaimsLoginUserData();
            // トップ画面モデル取得
            TopModel topModel = await GetTopModel(user.MainDepoID);
            return View(topModel);
        }

        /// <summary>
        /// トップ画面モデル取得
        /// </summary>
        /// <returns></returns>
        public async Task<TopModel> GetTopModel(int depoID)
        {
            TopModel topModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                // 最新のステーション状況取得SQL作成
                var latestStationStatusSQL = TopConnectController.CreateSQLToSelectLatestStationStatus(depoID);
                // 最新のステーション状況取得
                List<ViewCardModel> viewCardModelList = ConnectToSQLServer.ExecuteQueryToList<ViewCardModel>(latestStationStatusSQL);
                // トラック有無取得SQL作成
                var isExistTrucksSQL = TopConnectController.CreateSQLToSelectIsExistTrucksPerStationID();
                // トラック有無取得
                IEnumerable<ViewCardModel> isExistTrucksList = ConnectToSQLServer.ExecuteQueryToList<ViewCardModel>(isExistTrucksSQL);
                foreach (var item in viewCardModelList)
                {
                    var isExistTruck = isExistTrucksList.Where(x => x.StationID == item.StationID).ToList();
                    if (isExistTruck.Count == 1)
                    {
                        item.TruckExist = isExistTruck[0].TruckExist;
                    }
                }
                // 取得値の変換
                viewCardModelList = await ConversionOfGetValues(viewCardModelList);
                // ステーションの画像取得
                viewCardModelList = GetStationImage(viewCardModelList);
                topModel.ViewCardModelList = viewCardModelList;
                // ログインユーザーのメインデポ情報取得
                topModel.MainDepoID = user.MainDepoID;
                topModel.MainDepoName = user.MainDepoName;
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
        private async Task<List<ViewCardModel>> ConversionOfGetValues(List<ViewCardModel> models)
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

                // 表示する画像をAPIから取得してbase64に変換
                var imageUrl = ($"http://{model.IPAdress}/jpg/image.jpg");
                var imagePath64 = await GetImageBase64FromAPI(imageUrl);
                model.ImageBase64 = "data:image/jpeg;base64," + imagePath64;
            }
            return models;
        }

        /// <summary>
        /// ステーションの画像取得
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private List<ViewCardModel> GetStationImage(List<ViewCardModel> models)
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
        /// Base64でデコード、その後画像に変換できるか判定する
        /// </summary>
        /// <param name="imageBase64">Base64変換文字列</param>
        /// <returns></returns>        
        public bool CanDecodeImageBase64(string imageBase64)
        {
            try
            {
                // Base64でデコードできるか
                string base64String = imageBase64.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64String);
                // デコードしたものを画像に変換できるか
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 画像をAPIから取得する
        /// </summary>
        /// <param name="url">APIのurl</param>
        /// <returns></returns>
        public async Task<string> GetImageBase64FromAPI(string url)
        {
            var client = GetDigestClient(url);
            // 取得できない場合、空文字を返す
            try
            {
                var result = await client.GetAsync(url);
                var imageBytes = await result.Content.ReadAsByteArrayAsync();
                return Convert.ToBase64String(imageBytes);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// HttpClientにダイジェスト認証を設定する
        /// </summary>
        /// <param name="url">digest認証のurl</param>
        /// <returns></returns>
        private HttpClient GetDigestClient(string url)
        {
            //'CredentialCacheの作成
            var cache = new System.Net.CredentialCache();
            //'Digest認証の情報を追加
            cache.Add(new Uri(url), "Digest", new System.Net.NetworkCredential("root", "password"));

            var myClientHandler = new HttpClientHandler();
            myClientHandler.Credentials = cache;

            var client = new HttpClient(myClientHandler);
            client.Timeout = new TimeSpan(0, 0, 0, 0, 5000);
            return client;
        }
    }
}
