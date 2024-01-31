using Microsoft.AspNetCore.Mvc;
using mar_sumaken_web.Models;

namespace mar_sumaken_web.Controllers
{

    public class BaseController : Controller
    {
        // ログイン中ユーザー情報取得
        public LoginUserModel ClaimsLoginUserData()
        {
            var claimsLoginUserList = User.Claims.ToList();
            try
            {
                var loginUserModel = new LoginUserModel
                {
                    CompanyID = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "CompanyID").First().Value),
                    CompanyCode = claimsLoginUserList.Where(x => x.Type == "CompanyCode").First().Value,
                    CompanyName = claimsLoginUserList.Where(x => x.Type == "CompanyName").First().Value,
                    DatabaseName = claimsLoginUserList.Where(x => x.Type == "DatabaseName").First().Value,
                    UserID = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "UserID").First().Value),
                    UserName = claimsLoginUserList.Where(x => x.Type == "UserName").First().Value,
                    Role = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "Role").First().Value),
                    MainDepoID = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "MainDepoID").First().Value),
                    MainDepoName = claimsLoginUserList.Where(x => x.Type == "MainDepoName").First().Value,
                    AuthorizedKubun = Convert.ToInt32(claimsLoginUserList.Where(x => x.Type == "AuthorizedKubun").First().Value),
                    TimeStamp = Convert.ToDateTime(claimsLoginUserList.Where(x => x.Type == "TimeStamp").First().Value)
                };
                return loginUserModel;
            }
            catch(Exception)
            {
                throw;
            }
        }

    }

}