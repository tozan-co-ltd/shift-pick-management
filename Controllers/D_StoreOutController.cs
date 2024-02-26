using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出庫実績照会・修正画面
    /// </summary>
    public class D_StoreOutController : BaseController
    {
        /// <summary>
        /// 出庫実績照会・修正画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            D_StoreOutModel model = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 会社リスト取得
                CommonModel commonModel = new();
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_SupplierID);

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        /// <summary>
        /// 検索ボタン押下
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult SearchData(D_StoreOutModel model)
        {
            var searchData = string.Empty;

            try
            {
                // SQL作成
                var sql = D_StoreOutConnectController.CreateSQLToGetDStoreOuts(model);

                // DB接続
                List<D_StoreOutModel> dStoreOutList = D_StoreOutConnectController.ConnectDStoreOuts(sql, ClaimsLoginUserData().DatabaseName);

                // 表示用のhtml作成
                if (dStoreOutList.Count > 0)
                {
                    IEnumerable<D_StoreOutModel> query = dStoreOutList.Select(s => s);
                    model.D_StoreOutList = query.ToPagedList();

                    foreach (var item in model.D_StoreOutList)
                    {
                        searchData += "<tr>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutDate + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "<td>" + @item.StoreOutID + "</td>" +
                            "</tr>";
                    }
                }
                   
                return Content(searchData);
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                return Content(exceptionMessage);
            }
        }
    }
}
