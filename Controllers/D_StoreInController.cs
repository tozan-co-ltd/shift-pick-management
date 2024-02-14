using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace mar_sumaken_web.Controllers
{
    public class D_StoreInController : Controller
    {
        /// <summary>
        /// 入庫 - 入荷実績照会
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(D_StoreInModel model)
        {
            if (model == null)
                model = new D_StoreInModel();

            return View(model);
        }
    }
}
