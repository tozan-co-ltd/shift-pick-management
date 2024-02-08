using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    public class ImportShipmentScheduleController : BaseController
    {
        /// <summary>
        /// 出荷指示取込画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var model = new D_FileImportModel();

                var listD_FileImport = GetListD_FileImport(ClaimsLoginUserData().DatabaseName);

                if (listD_FileImport.Count > 0)
                {
                    IEnumerable<D_FileImportModel> query = listD_FileImport.Select(s => s);
                    model.D_FileImportList = query.ToPagedList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // log取得
                var exceptionMessage = ex.Message;

                var d_FileImportModel = new D_FileImportModel
                {
                    Message = exceptionMessage
                };
                return View(d_FileImportModel);
            }
        }


        /// <summary>
        /// 出荷指示取込一覧取得
        /// </summary>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<D_FileImportModel> GetListD_FileImport(string databaseName)
        {
            // SQL作成
            var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport();

            // DB接続
            List<D_FileImportModel> strList = D_FileImportConnectController.ConnectD_FileImport(sql, databaseName);

            return strList;
        }
    }
}
