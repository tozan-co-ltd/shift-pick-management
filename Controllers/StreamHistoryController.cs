using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class StreamHistoryController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new StreamHistoryModel
            {
                UserName = user?.UserName ?? string.Empty,
                Acknowledged = false,
                OccurredAt = System.DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Acknowledge(StreamHistoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.Acknowledged = true;
            model.AcknowledgedAt = System.DateTime.Now;

            return View("Index", model);
        }
    }
}
