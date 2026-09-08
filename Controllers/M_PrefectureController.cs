using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class M_PrefectureController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new M_PrefectureModel
            {
                UserName = user?.UserName ?? string.Empty,
                Registered = false
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Register(M_PrefectureModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.Registered = true;
            model.RegisteredAt = System.DateTime.Now;

            return View("Index", model);
        }
    }
}
