using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class AIModelDescriptionController : Controller
    {
        public IActionResult Index()
        {
            var model = new AIModelDescriptionViewModel();
            var sql = AIModelDescriptionConnectController.CreateSQLToSelectAIModelDescriptions();
            model.DescriptionList = ConnectToSQLServer.ExecuteQueryToList<AIModelDescriptionModel>(sql);
            return View(model);
        }
    }
}
