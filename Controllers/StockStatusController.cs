using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class StockStatusController : BaseController
    {
        /// <summary>
        /// 入庫 - 入荷予定照会
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(StockStatusModel model)
        {
            if (model == null)
                model = new StockStatusModel();

            return View(model);
        }
    }
}
