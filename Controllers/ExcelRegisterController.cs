using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class ExcelRegisterController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new ExcelRegisterModel
            {
                UserName = user?.UserName ?? string.Empty,
                Registered = false
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Register(ExcelRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Placeholder: actual file processing/persistence can be added later.
            model.Registered = true;
            model.RegisteredAt = System.DateTime.Now;

            return View("Index", model);
        }
    }
}
