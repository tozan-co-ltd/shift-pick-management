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
        public IActionResult Index(D_StoreOutModel.D_StoreOut model)
        {
            if (model == null)
                model = new D_StoreOutModel.D_StoreOut();

            return View(model);
        }


        /// <summary>
        /// 検索実装
        /// </summary>
        /// <param name="model">model</param>
        /// <returns>倉庫マスター情報</returns>
        public IActionResult SearchData(D_StoreOutModel.D_StoreOut model)
        {
            string? errorMessage;

            try
            {
                var listD_StoreOut = GetListD_StoreOut(model, ClaimsLoginUserData().DatabaseName);
                var searchData = string.Empty;
                if (listD_StoreOut.Count > 0)
                {
                    IEnumerable<D_StoreOutModel.D_StoreOut> query = listD_StoreOut.Select(s => s);
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
                // log取得
                var exceptionMessage = ex.Message;
     
                return Content(exceptionMessage);
            }
        }


        /// <summary>
        /// 在庫 - 出庫一覧データを取得
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="databaseName">string</param>
        /// <returns> 在庫 - 出庫情報</returns>
        public List<D_StoreOutModel.D_StoreOut> GetListD_StoreOut(D_StoreOutModel.D_StoreOut model, string databaseName)
        {
            // SQL作成
            var sql = D_StoreOutConnectController.CreateSQLToGetD_StoreOutList(model);

            // DB接続
            List<D_StoreOutModel.D_StoreOut> strList = D_StoreOutConnectController.ConnectD_StoreOut(sql, databaseName);

            return strList;
        }
    }
}
