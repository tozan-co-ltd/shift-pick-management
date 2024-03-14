using System.Data.SqlClient;
using mar_sumaken_web.Commons;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Dapper;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 共通Model
    /// </summary>
    /// <remarks>画面表示に必要な情報(ログイン中ユーザーのClaim,メニュー名,検索用倉庫名・会社名)を取得</remarks>
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
        /// 管理権限区分
        /// </summary>
        public int AuthorizedKubun { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Controller名
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
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

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
            AuthorizedKubun = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_AuthorizedKubun).First().Value);
            ControllerName = viewContext.RouteData.Values["controller"].ToString();
            CategoryTitle = GetCategoryTitle();
            ViewTitle = GetViewTitle();
            MDepoList = GetMDepoList(DataBaseName);
            SelectedDepoID = Convert.ToInt32(claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_MainDepoID).First().Value);
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
        public IEnumerable<SelectListItem> GetMDepoList(string databaseName)
        {
            var selectListItem = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                            DepoID as Value,
                            DepoName AS Text
                        FROM M_Depo
                        WHERE (1=1)
                            AND IsDeleted = 0
                        ";

                    selectListItem = connection.Query<SelectListItem>(commandText).ToList();
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
        public IEnumerable<SelectListItem> GetMCompanyList(string databaseName, int companyKubun)
        {
            var companyList = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = $@"
                        SELECT 
                            CompanyID AS Value
                            ,CASE 
	                            WHEN TRIM(ClientName) = '' THEN CompanyName
	                            WHEN ClientName IS NULL THEN CompanyName
                                ELSE concat(CompanyName, ' - ', ClientName) 
                            END AS Text
                        FROM M_Company
                        WHERE (1=1)
                            AND CompanyKubun = {companyKubun}
                            AND IsDeleted = 0
                     ";

                    // 会社名(仕入先名/納入先名)のセレクトボックスに「会社名 - 得意先名」と表示させる
                    companyList = connection.Query<SelectListItem>(sql).ToList();
                }

                return companyList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
