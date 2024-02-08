using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace mar_sumaken_web.Controllers
{
    public class D_StoreInController : Controller
    {
        public IActionResult Index(D_StoreInModel model)
        {
            if (model == null)
                model = new D_StoreInModel();

            return View(model);
        }
    }
}
