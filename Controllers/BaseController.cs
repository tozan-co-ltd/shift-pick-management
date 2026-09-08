using Microsoft.AspNetCore.Mvc;
using shift_pick_management.Models;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Commons;

namespace shift_pick_management.Controllers
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
                    Int32.TryParse(claimsLoginUserList.Where(x => x.Type == "AuthorizedKubun").First().Value, out var authorizedKubun);
                    var loginUserModel = new LoginUserModel
                    {
                        AuthorizedKubun = authorizedKubun,
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