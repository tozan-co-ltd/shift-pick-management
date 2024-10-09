using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using static ai_truck_load_measurement.Models.TopModel;

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
                    var isExistTruck = isExistTrucksList.Where(x =>x.StationID == item.StationID).ToList();
                    if(isExistTruck.Count != 1)
                    {
                        ViewData["ErrorMessage"] = "E3004: " + ErrorMessagesResources.E3004;
                        return View(topModel);
                    }
                    item.TruckExist = isExistTruck[0].TruckExist;
                }
                // 取得値の変換
                topModelList = ConversionOfGetValues(topModelList);
                // ステーションの画像取得
                topModelList = GetStationImage(topModelList);
                topModel.TopModelList = topModelList;
                return View(topModel);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(topModel);
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
                // 道路クラスと車両の存在有無により分岐
                var loadClass = model.LoadClass;
                var truckExist = model.TruckExist;
                var truckStatus = string.Empty;
                if (loadClass == -1 && !truckExist)
                {
                    truckStatus = "空車";
                }
                else if (loadClass == -1 && truckExist || loadClass == 0 || loadClass == 1)
                {
                    truckStatus = "停車";
                }
                else if (loadClass == 2)
                {
                    truckStatus = "停車, 0%";
                }
                else if (loadClass == 3)
                {
                    truckStatus = "停車, 1~10%";
                }
                else if (loadClass == 4)
                {
                    truckStatus = "停車, 11~20%";
                }
                else if (loadClass == 5)
                {
                    truckStatus = "停車, 21~30%";
                }
                else if (loadClass == 6)
                {
                    truckStatus = "停車, 31~40%";
                }
                else if (loadClass == 7)
                {
                    truckStatus = "停車, 41~50%";
                }
                else if (loadClass == 8)
                {
                    truckStatus = "停車, 51~60%";
                }
                else if (loadClass == 9)
                {
                    truckStatus = "停車, 61~70%";
                }
                else if (loadClass == 10)
                {
                    truckStatus = "停車, 71~80%";
                }
                else if (loadClass == 11)
                {
                    truckStatus = "停車, 81~90%";
                }
                else if (loadClass == 12)
                {
                    truckStatus = "停車, 91~100%";
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
                // 画像パスに画像がない場合はダミー画像を表示する
                if (string.IsNullOrEmpty(model.ImagePath))
                {
                    model.ImagePath = "\"V:\\data\\system\\企業別\\T011_東山\\システム部\\AI荷量把握改善2024\\10_仕様書\\05_詳細設計書\\images\\NoImage.png\"";
                }
            }
            return models;
        }
    }
}
