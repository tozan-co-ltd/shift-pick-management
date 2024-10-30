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
                        DatabaseName = claimsLoginUserList.Where(x => x.Type == "DatabaseName").First().Value,
                        UserName = claimsLoginUserList.Where(x => x.Type == "UserName").First().Value,
                        Role = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "Role").First().Value),
                        TimeStamp = Convert.ToDateTime(claimsLoginUserList.Where(x => x.Type == "TimeStamp").First().Value)
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