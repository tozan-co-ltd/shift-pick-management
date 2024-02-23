using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 倉庫マスター画面
    /// </summary>
    public class M_DepoController : BaseController
    {
        /// <summary>
        /// 倉庫マスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var model = new M_DepoModel();

                var listM_Depo = GetListM_Depo(ClaimsLoginUserData().DatabaseName);

                if (listM_Depo.Count > 0)
                {
                    IEnumerable<M_DepoModel> query = listM_Depo.Select(s => s);
                    model.M_DepoList = query.ToPagedList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // log取得
                var exceptionMessage = ex.Message;

                var M_DepoModel = new M_DepoModel
                {
                    Message = exceptionMessage
                };
                return View(M_DepoModel);
            }
        }


        /// <summary>
        /// 出荷指示取込一覧取得
        /// </summary>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<M_DepoModel> GetListM_Depo(string databaseName)
        {
            // SQL作成
            var sql = M_DepoConnectController.CreateSQLToGetMDepoList();

            // DB接続
            List<M_DepoModel> strList = M_DepoConnectController.ConnectMDepo(sql, databaseName);

            return strList;
        }
    }
}
