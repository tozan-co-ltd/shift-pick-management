using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    public class D_StoreOutController : BaseController
    {
        /// <summary>
        /// 出庫実績照会・修正画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(D_StoreOutModel model)
        {
            if (model == null)
                model = new D_StoreOutModel();

            return View(model);
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
                var sql = D_StoreOutConnectController.CreateSQLToGetDStoreOut(model);

                // DB接続
                List<D_StoreOutModel> listD_StoreOut = D_StoreOutConnectController.ConnectD_StoreOut(sql, ClaimsLoginUserData().DatabaseName);

                // 表示用のhtml作成
                if (listD_StoreOut.Count > 0)
                {
                    IEnumerable<D_StoreOutModel> query = listD_StoreOut.Select(s => s);
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
