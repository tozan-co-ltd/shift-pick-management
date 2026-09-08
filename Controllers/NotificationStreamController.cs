using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class NotificationStreamController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new NotificationStreamModel
            {
                UserName = user?.UserName ?? string.Empty,
                Confirmed = false
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Confirm(NotificationStreamModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.Confirmed = true;
            model.ConfirmedAt = System.DateTime.Now;

            return View("Index", model);
        }
    }
}
