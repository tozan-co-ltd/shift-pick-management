//using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Controllers
{
    public class M_UserController : Controller
    {
        private readonly ILogger<M_UserController> _logger;

        public M_UserController(ILogger<M_UserController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index(M_UserModel model)
        {
            string? errorMessage;

            if (model == null)
                model = new M_UserModel();

            try
            {
                List<M_User> users = new List<M_User> ();

                for (int i = 4; i < 100; i++)
                {
                    M_UserModel.M_User item = new M_UserModel.M_User
                    { 
                        UserId = i, LoginId = "sfsd", UserName = "User name " + i,  DepoId = 1, AuthorizedKubun = 1, UpdatedAt = DateTime.Now };
                        users.Add(item);
                    }

                if (users.Count > 0)
                {
                    IEnumerable<M_User> query = users.Select(s => s);
                    model.MUserList = query.ToPagedList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
    }
}
