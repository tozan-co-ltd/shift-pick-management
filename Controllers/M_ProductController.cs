using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class M_ProductController : BaseController
    {
        private readonly ILogger<M_ProductController> _logger;

        public M_ProductController(ILogger<M_ProductController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 商品マスター情報取得
        /// </summary>
        public IActionResult Index()
        {
            //List<M_ProductModel> listProduct = new List<M_ProductModel>();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    ViewData["ErrorMessage"] = ErrorMessagesResources.E2001;
                    return View();
                }

                // 商品マスター情報取得SQL作成
                var sql = M_ProductConnectController.CreateSQLToSelectMProducts();
                // DB接続
                List<M_ProductModel> productList = M_ProductConnectController.ConnectMProducts(sql, user.DatabaseName);
                if (productList.Count > 0)
                {
                    // 倉庫-品番中間取得
                    productList = M_ProductConnectController.GetRDepoProducts(productList, user.DatabaseName);
                }

                return View(productList);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View();
            }
        }

        /// <summary>
        /// 品番マスターを削除
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns></returns>
        public IActionResult Delete(int productId)
        {
            try
            {

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                if (user == null || productId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // 品番マスター削除
                int deleteAffectedRows = M_ProductConnectController.DeleteMProduct(productId, user.DatabaseName);

                // 更新件数が0の場合はエラーとする
                if (deleteAffectedRows == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }
    }
}
