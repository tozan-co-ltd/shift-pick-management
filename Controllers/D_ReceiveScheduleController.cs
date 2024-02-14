using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace mar_sumaken_web.Controllers
{
    public class D_ReceiveScheduleController : BaseController
    {
         public IActionResult Index(D_ReceiveScheduleModel model)
        {
            if (model == null)
                model = new D_ReceiveScheduleModel();

            return View(model);
        }
    }
}
