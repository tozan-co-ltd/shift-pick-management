using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    public class TopController : Controller
    {
        private readonly ILogger<TopController> _logger;

        public TopController(ILogger<TopController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// トップ画面表示
        /// </summary>
        public IActionResult Index(D_HandyErrorMessageModel model)
        {
            string? errorMessage;

            if (model == null)
                model = new D_HandyErrorMessageModel();

            try
            {
                List<D_HandyErrorMessageModel> handyErrors = new List<D_HandyErrorMessageModel>
                {
                    new D_HandyErrorMessageModel { HandyErrorMessageID = 1, ReadingTime = DateTime.Now, HandyMenuName = "出庫", ErrorMessage = "E1001:既にスキャン済みの納入先かんばんです。", CreatedBy = "田中次郎" },
                    new D_HandyErrorMessageModel { HandyErrorMessageID = 2, ReadingTime = DateTime.Now, HandyMenuName = "まとめ入庫", ErrorMessage = "E1002:品番マスターに一致する品番がありません。", CreatedBy = "田中次郎" },
                    new D_HandyErrorMessageModel { HandyErrorMessageID = 3, ReadingTime = DateTime.Now, HandyMenuName = "…", ErrorMessage = "…" , CreatedBy = "田中次郎" },
                    new D_HandyErrorMessageModel { HandyErrorMessageID = 4, ReadingTime = DateTime.Now, HandyMenuName = "…", ErrorMessage = "…" , CreatedBy = "田中次郎" },
                    new D_HandyErrorMessageModel { HandyErrorMessageID = 5, ReadingTime = DateTime.Now, HandyMenuName = "…", ErrorMessage = "…" , CreatedBy = "田中次郎" },
                };

                if (handyErrors.Count > 0)
                {
                    IEnumerable<D_HandyErrorMessageModel> query = handyErrors.Select(s => s);
                    model.D_HandyErrorMessageList = query.ToPagedList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new D_HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
                return View(model);
            }
        }

    }
}
