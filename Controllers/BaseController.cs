using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.ConnectControllers;

namespace ai_truck_load_measurement.Controllers
{

    public class BaseController : Controller
    {
        // ログイン中ユーザー情報取得
        public LoginUserModel? ClaimsLoginUserData()
        {
            var claimsLoginUserList = User.Claims.ToList();
            try
            {
                if (claimsLoginUserList.Count > 0)
                {
                    Int32.TryParse(claimsLoginUserList.Where(x => x.Type == "MainDepoID").First().Value, out var mainDepoID);
                    var loginUserModel = new LoginUserModel
                    {
                        UserName = claimsLoginUserList.Where(x => x.Type == "UserName").First().Value,
                        ADName = claimsLoginUserList.Where(x => x.Type == "ADName").First().Value,
                        MainDepoName = claimsLoginUserList.Where(x => x.Type == "MainDepoName").First().Value,
                        MainDepoID = mainDepoID,
                    };
                    return loginUserModel;
                }
                return null;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }

}