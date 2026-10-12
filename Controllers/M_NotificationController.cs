using shift_pick_management.Models;
using Microsoft.AspNetCore.Mvc;

namespace shift_pick_management.Controllers
{
    public class M_NotificationController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new M_NotificationModel
            {
                UserName = user?.UserName ?? string.Empty,
                Confirmed = false
            };

            return View(model);
        }
    }
}
