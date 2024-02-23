using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// トップ画面
    /// </summary>
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
        public IActionResult Index()
        {
            var model = new D_HandyErrorMessageModel.D_HandyErrorMessage();
            return View(model);
        }
    }
}
