using shift_pick_management.Commons;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using Microsoft.AspNetCore.Mvc;

namespace shift_pick_management.Controllers
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
