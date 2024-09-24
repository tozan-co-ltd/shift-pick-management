using Microsoft.AspNetCore.Mvc;
using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using ai_truck_load_measurement.Commons;
using System.ComponentModel.Design;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using ai_truck_load_measurement.ConnectControllers;

namespace ai_truck_load_measurement.Filters
{

    public class AccessControlFilter : IActionFilter
    {
        /// <summary>
        /// アクセス制御
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>コントローラー アクションが実行される前に呼び出される</remarks>
        /// <exception cref="NotImplementedException"></exception>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 1.ログイン中ユーザーの管理権限区分によって許可されていないページに
            //   アクセスしようとした場合はエラー表示 / ボタン非表示

            // アクセス制御しないController名
            string[] src = { "account", "home", "login", "top" };
            var list = new List<string>();
            list.AddRange(src);

            // アクセスするController名取得
            var accessController = context.RouteData.Values["controller"].ToString().ToLower();

            int num = list.IndexOf(accessController);
            if (num != -1)
            {
                // アクセス制御しない
                Console.WriteLine(accessController);
            }
            else
            {
                // 表示しているメニューのController名一覧を取得
                WebMenuModel webMenuModel = new()
                {
                    CompanyID = int.Parse(context.HttpContext.User.FindFirstValue(CustomClaimTypes.ClaimType_CampanyID)),
                    Role = int.Parse(context.HttpContext.User.FindFirstValue(CustomClaimTypes.ClaimType_Role))
                };
                var viewMenuList = webMenuModel.MenuList(null);

                // アクセスするController名が表示しているメニューに含まれている場合はアクセス制御しない
                bool IsAccessible = false;
                foreach (var viewMenu in viewMenuList)
                {
                    var viewController = viewMenu.Controller.ToLower();

                    if (accessController == viewController)
                    {
                        IsAccessible = true;
                        break;
                    };
                }

                // アクセスするController名が表示しているメニューに含まれていない場合はアクセス拒否ページ(Shared/AccessDenied)へ遷移
                //if (!IsAccessible)
                //{
                //    var viewResult = new ViewResult
                //    {
                //        ViewName = "AccessDenied"
                //    };
                //    context.Result = viewResult;
                //    return;
                //}

                //// 2.同一ユーザーによる複数端末での同時ログインを禁止する
                ////   先にログインしたユーザーがログアウト(ログイン画面へリダイレクト)される
                //var controller = context.Controller as Controller;
                //var databaseName = controller.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;

                //// ログイン中ユーザー情報取得
                //var loginUserModel = new LoginUserModel();
                //var userID = Convert.ToInt32(context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value);
                //var timeStamp = context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_TimeStamp).First().Value.ToString();

                //// Claimsのタイムスタンプとユーザーマスターの最終ログイン日時が一致しない場合は強制ログアウト
                //// 最終ログイン日時が一致するユーザー情報取得
                //var IsMatched = false;
                //if (DateTime.TryParse(timeStamp, out DateTime lastLoginDatetime))
                //{
                //    // SQL作成
                //    var sql = LoginConnectController.CreateSQLToSelectMUserByLastLoginDatetime(userID, lastLoginDatetime);
                //    // DB接続
                //    IsMatched = M_UserConnectController.ConnectMUserWithMatchingLastLoginDatetime(sql, databaseName);
                //}

                //// 一致するユーザー情報がない場合はログイン画面へリダイレクト
                //if (!IsMatched)
                //{
                //    var viewResult = controller.RedirectToAction("Logout", "Login", new { param = "autologout" });
                //    context.Result = viewResult;
                //    return;
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>コントローラー アクションの実行後に呼び出される</remarks>
        /// <exception cref="NotImplementedException"></exception>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new NotImplementedException();
        }
    }
}