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
            if (claimsLoginUserList.Count > 0)
            {
                Int32.TryParse(claimsLoginUserList.Where(x => x.Type == "AuthorizedKubun").First().Value, out var authorizedKubun);
                var loginUserModel = new LoginUserModel
                {
                    UserName = claimsLoginUserList.Where(x => x.Type == "UserName").First().Value,
                    ADName = claimsLoginUserList.Where(x => x.Type == "ADName").First().Value,
                    AuthorizedKubun  = authorizedKubun,
                };
                return loginUserModel;
            }
            return null;
        }

        public M_DepoModel GetMainDepo()
        {
            M_DepoModel model = new M_DepoModel();
            var user = ClaimsLoginUserData();
            var sql = M_DepoConnectController.CreateSQLToSelectDepoFromADName(user.ADName);
            var depoList = M_DepoConnectController.ConnectMDepos(sql);
            if (depoList.Count > 0)
            {
                model = depoList[0];
            }
            return model;
            
        }

    }

}