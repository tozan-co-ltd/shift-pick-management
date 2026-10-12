using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using shift_pick_management.Commons;
using System.Data;
using System.DirectoryServices;
using NPOI.SS.Formula.Functions;

namespace shift_pick_management.Controllers
{
    public class M_UserController : BaseController
    {
        /// <summary>
        /// ユーザーマスター画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            M_UserListViewModel model = new();

            try
            {
                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToSelectMUsers();
                // DB接続
                IEnumerable<M_UserModel> userList = ConnectToSQLServer.ExecuteQueryToList<M_UserModel>(sql);

                model.M_UserList = userList.ToPagedList();
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }

    }
}
