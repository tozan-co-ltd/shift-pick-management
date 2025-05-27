using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class NewsController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            NewsListViewModel model = new();
            var sql = NewsConnectController.CreateSQLToSelectNews();
            model.NewsList = ConnectToSQLServer.ExecuteQueryToList<NewsModel>(sql);
            model.AuthorizedKubun = user.AuthorizedKubun;
            foreach(var news in model.NewsList)
            {
                news.CategoryStatus = ConversionCategoryClassToCategoryStatus(news);
            }
            return View(model);
        }

        /// <summary>
        /// カテゴリークラスからカテゴリー名への変換
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        private string ConversionCategoryClassToCategoryStatus(NewsModel news)
        {
            var categoryClass = news.CategoryClass;
            var categoryStatus = "";
            if (categoryClass == 0)
                categoryStatus = "アップデート";
            else if (categoryClass == 1)
                categoryStatus = "メンテナンス";
            else if (categoryClass == 2)
                categoryStatus = "障害・不具合";
            else if (categoryClass == 3)
                categoryStatus = "操作案内";

            return categoryStatus;
        }

        /// <summary>
        /// お知らせ登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(NewsModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"お知らせ登録失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                model.NewsContent = model.NewsContent.Replace("\r\n", "<br />");

                // お知らせ登録
                var sql = NewsConnectController.CreateSQLToInsertNews(model, DateTime.Now, user.UserName);
                ConnectToSQLServer.ExecuteQuery(sql);

                // log取得
                _logger.Info($"お知らせ登録成功");

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
        /// お知らせ詳細画面表示
        /// </summary>
        /// <param name="newsID"></param>
        /// <returns></returns>
        public IActionResult Detail(int newsID)
        {
            NewsListViewModel model = new();
          
            try
            {
                var sql = NewsConnectController.CreateSQLToSelectNewsFromNewsID(newsID);
                model.NewsList =  ConnectToSQLServer.ExecuteQueryToList<NewsModel>(sql);
                foreach (var news in model.NewsList)
                {
                    news.CategoryStatus = ConversionCategoryClassToCategoryStatus(news);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }
    }
}
