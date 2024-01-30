using Microsoft.AspNetCore.Mvc;
using mar_sumaken_web.Models;

namespace mar_sumaken_web.Controllers
{

    public class BaseController : Controller
    {
        // ログイン中ユーザー情報取得
        public LoginUserModel UserDataList()
        {
            var userData = User.Claims.ToList();
            try
            {
                var userModel = new LoginUserModel
                {
                    CompanyID = Convert.ToInt32(userData.Where(x => x.Type == "CompanyID").First().Value),
                    CompanyCode = userData.Where(x => x.Type == "CompanyCode").First().Value,
                    CompanyName = userData.Where(x => x.Type == "CompanyName").First().Value,
                    DatabaseName = userData.Where(x => x.Type == "DatabaseName").First().Value,
                    UserID = Convert.ToInt32(userData.Where(x => x.Type == "UserID").First().Value),
                    UserName = userData.Where(x => x.Type == "UserName").First().Value,
                    Role = Convert.ToInt32(userData.Where(x => x.Type == "Role").First().Value),
                    MainDepoID = Convert.ToInt32(userData.Where(x => x.Type == "MainDepoID").First().Value),
                    MainDepoName = userData.Where(x => x.Type == "MainDepoName").First().Value,
                    AuthorizedKubun = Convert.ToInt32(userData.Where(x => x.Type == "AuthorizedKubun").First().Value),
                    TimeStamp = Convert.ToDateTime(userData.Where(x => x.Type == "TimeStamp").First().Value)
                };
                return userModel;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

    }

}