using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadProgressController : BaseController
    {
        public IActionResult Index()
        {
            LoadProgressModel model = new();
            var user = ClaimsLoginUserData();
            model.SelectedDepo = new M_DepoModel { 
                DepoID = user.MainDepoID,
                Name = user.MainDepoName
            };

            return View(model);
        }

    }
}
