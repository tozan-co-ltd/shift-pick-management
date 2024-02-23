using System.Data.SqlClient;
using mar_sumaken_web.Commons;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Dapper;

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
        public string? DataBaseName { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? ControllerName { get; set; }

        /// <summary>
        /// ビュータイトル
        /// </summary>
        public string? ViewTitle { get; set; }

        /// <summary>
        /// カテゴリータイトル
        /// </summary>
        public string? CategoryTitle { get; set; }

        /// <summary>
        /// 倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem>? MDepoList { get; set; }

        /// <summary>
        /// 会社リスト
        /// </summary>
        public IEnumerable<SelectListItem>? MCompanyList { get; set; }


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
            CategoryTitle = GetCategoryTitle();
            ViewTitle = GetViewTitle();
            MDepoList = GetMDepoList();
        }

        /// <summary>
        /// ページタイトル(カテゴリー名)取得
        /// </summary>
        /// <returns>カテゴリー名</returns>
        public string GetCategoryTitle()
        {
            string categoryTitle = "";

            try
            {
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionStringForMaster();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                              SELECT
	                                A.CategoryName AS CategoryName
                              FROM M_WebMenuCategory AS A
                              LEFT OUTER JOIN 
                                    M_WebMenuController AS B ON (A.CategoryID = B.CategoryID)
                              LEFT OUTER JOIN 
                                    M_WebMenu AS C ON (C.CategoryID = B.CategoryID AND C.MenuID = B.MenuID)
                              WHERE 1=1
                                  AND C.CompanyID   = @CompanyID
                                  AND B.Controller  = @Controller
                        ";

                     var param = new
                    {
                        CompanyID = CompanyID,
                        Controller = ControllerName
                    };
                    categoryTitle = connection.ExecuteScalar<string>(commandText, param);
                    return categoryTitle;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ページタイトル(WEBメニュー名)取得
        /// </summary>
        /// <returns>WEBメニュー名</returns>
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
	                                A.MenuName          AS MenuName
                              FROM M_WebMenu            AS A
                              LEFT OUTER JOIN 
                                    M_WebMenuController AS B 
                                    ON (A.CategoryID = B.CategoryID AND A.MenuID = B.MenuID)
                              WHERE 1=1
                                    AND A.CompanyID     = @CompanyID
                                    AND B.Controller    = @Controller
                              ORDER BY SortNumber Asc
                        ";

                    var param = new
                    {
                        CompanyID = CompanyID,
                        Controller = ControllerName
                    };
                    pageTitle = connection.ExecuteScalar<string>(commandText, param);
                    return pageTitle;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 倉庫リスト取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetMDepoList()
        {
            var selectListItem = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(DataBaseName);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT *
                        FROM M_Depo
                        WHERE (1=1)
                            AND IsDeleted = 0
                        ";
                    var param = new
                    {
                        UserID = UserID
                    };

                    var depoList = new List<M_DepoModel>();
                    depoList = connection.Query<M_DepoModel>(commandText, param).ToList();

                    foreach (var depo in depoList)
                    {
                        var item = new SelectListItem { 
                            Value = depo.DepoID.ToString(), 
                            Text = depo.DepoName.ToString() 
                        };
                        selectListItem.Add(item);
                    }
                }
                return selectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社リスト取得
        /// </summary>
        /// <returns></returns>
        //public IEnumerable<SelectListItem> GetMCompanyList(int companyKubun)
        //{
        //    var selectListItem = new List<SelectListItem>();

        //    try
        //    {
        //        // SQLServer接続文字列取得
        //        var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(DataBaseName);
        //        // SQLServer接続
        //        using (var connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            var sql = $@"
        //                SELECT *
        //                FROM M_Company
        //                WHERE (1=1)
        //                    AND CompanyKubun = {companyKubun}
        //                    AND IsDeleted = 0
        //                ";

        //            var companyList = new List<M_CompanyModel>();
        //            companyList = connection.Query<M_CompanyModel>(sql).ToList();

        //            // 会社名(仕入先名/納入先名)のセレクトボックスに「会社名 - 得意先名」と表示させる
        //            foreach (var company in companyList)
        //            {
        //                var item = new SelectListItem
        //                {
        //                    Value = company.CompanyID.ToString(),
        //                    Text = company.CompanyName.ToString() + " - " + company.ClientName.ToString()
        //                };
        //                selectListItem.Add(item);
        //            }
        //        }
        //        return selectListItem;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        /// <summary>
        /// 会社リスト取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetMCompanyList(string databaseName, int companyKubun)
        {
            var selectListItem = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = $@"
                        SELECT *
                        FROM M_Company
                        WHERE (1=1)
                            AND CompanyKubun = {companyKubun}
                            AND IsDeleted = 0
                        ";

                    var companyList = new List<M_CompanyModel>();
                    companyList = connection.Query<M_CompanyModel>(sql).ToList();

                    // 会社名(仕入先名/納入先名)のセレクトボックスに「会社名 - 得意先名」と表示させる
                    foreach (var company in companyList)
                    {
                        var item = new SelectListItem
                        {
                            Value = company.CompanyID.ToString(),
                            Text = company.CompanyName.ToString() + " - " + company.ClientName.ToString()
                        };
                        selectListItem.Add(item);
                    }
                }
                return selectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
