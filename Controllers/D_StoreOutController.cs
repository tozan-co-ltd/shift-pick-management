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
            string? errorMessage;

            try
            {
                var listD_StoreOut = GetListD_StoreOut(model, ClaimsLoginUserData().DatabaseName);
                var searchData = string.Empty;

                // 表示用のhtml作成
                if (listD_StoreOut.Count > 0)
                {
                    IEnumerable<D_StoreOutModel> query = listD_StoreOut.Select(s => s);
                    model.LstD_StoreOut = query.ToPagedList();

                    foreach (var item in model.LstD_StoreOut)
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


        /// <summary>
        /// 出庫実績情報取得
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<D_StoreOutModel> GetListD_StoreOut(D_StoreOutModel model, string databaseName)
        {
            // SQL作成
            var sql = D_StoreOutConnectController.CreateSQLToGetDStoreOut(model);

            // DB接続
            List<D_StoreOutModel> strList = D_StoreOutConnectController.ConnectD_StoreOut(sql, databaseName);

            return strList;
        }
    }
}
