using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class M_OfficeController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new M_OfficeModel
            {
                UserName = user?.UserName ?? string.Empty,
                Registered = false
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Register(M_OfficeModel model)
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
