using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;

namespace shift_pick_management.Controllers
{
    public class OuboConfirmationController : BaseController
    {
        /// <summary>
        /// Oubo (notification) confirmation index page
        /// </summary>
        /// <returns>View</returns>
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            var model = new OuboConfirmationModel
            {
                UserName = user?.UserName ?? string.Empty,
                Confirmed = false
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Confirm(OuboConfirmationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // For now just mark as confirmed. Persistence can be added later.
            model.Confirmed = true;
            model.ConfirmedAt = System.DateTime.Now;

            return View("Index", model);
        }
    }
}
