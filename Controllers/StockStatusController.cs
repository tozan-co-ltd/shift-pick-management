using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class StockStatusController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
