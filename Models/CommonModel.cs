using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using mar_sumaken_web.Commons;
using System.Threading.Tasks;
using System.CodeDom;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using mar_sumaken_web.Controllers;
using System.Security.Claims;
using Dapper;
using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace mar_sumaken_web.Models
{
    public class CommonModel
    {
        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// データベース名
        /// </summary>
        public string DataBaseName { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        public string ControllerName { get; set; }

        /// <summary>
        /// ビュータイトル
        /// </summary>
        public string ViewTitle { get; set; }


        /// <summary>
        /// ベースビュー作成
        /// </summary>
        /// <remarks>会社ID、データベース名、ユーザーID、ロール、コントロール名、ビュータイトルを設定</remarks>
        /// <param name="claimsPrincipal">ClaimsPrincipal</param>
        /// <param name="viewContext">ViewContext</param>
        public void GetBaseView(ClaimsPrincipal claimsPrincipal, ViewContext viewContext)
        {
            CompanyID = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CampanyID).First().Value);
            DataBaseName = claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_DatabaseName).First().Value;
            UserID = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_UserID).First().Value);
            Role = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_Role).First().Value);          
            ControllerName = viewContext.RouteData.Values["controller"].ToString();
            ViewTitle = GetViewTitle();
        }

        /// <summary>
        /// タイトルビューを取得
        /// </summary>
        /// <returns>ページのタイトル</returns>
        public string GetViewTitle()
        {
            string pageTitle = "";

            try
            {
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionStringForMaster();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                              SELECT
	                            A.MenuName AS MenuName
                              FROM M_WebMenu AS A
                              LEFT OUTER JOIN M_WebMenuController AS B ON  (A.CategoryID = B.CategoryID AND A.MenuID = B.MenuID)
                              WHERE 1=1
                                  AND A.CompanyID = @CompanyID
                                  AND B.Controller    = @Controller
                              ORDER BY SortNumber Asc
                        ";

                    var param = new
                    {
                        CompanyID = CompanyID,
                        Controller = ControllerName
                    };
                    pageTitle = connection.ExecuteScalar<string>(commandText, param);
                }
            }
            catch (Exception ex)
            {
                //DB取得エラー
                return pageTitle;
            }

            return pageTitle;
        }
    }
}
