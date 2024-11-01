using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;

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
                    var loginUserModel = new LoginUserModel
                    {
                        UserName = claimsLoginUserList.Where(x => x.Type == "UserName").First().Value
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