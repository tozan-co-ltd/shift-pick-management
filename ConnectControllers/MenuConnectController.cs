using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using System.ComponentModel.Design;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// メニュに関する関数
    /// </summary>
    public static class MenuConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="CompanyID"></param>
        /// <param name="categoryID"></param>
        /// <returns>ユーザー情報</returns>
        public static List<M_WebMenu> ConnectMenu(string sql, int CompanyID, int categoryID)
        {
            // 戻り値
            List<M_WebMenu> menuModels = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionStringForMaster();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    var param = new
                    {
                        CompanyID = CompanyID,
                        RoleFlag = 1,
                        CategoryID = categoryID
                    };

                    menuModels = connection.Query<M_WebMenu>(sql, param).ToList();
                }
                return menuModels;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// メニュSELECT文SQL作成
        /// </summary>
        /// <param name="userRoleName"></param>
        /// <param name="categoryID"></param>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMenu(string userRoleName, int categoryID)
        {
            string whereString = $@"AND (A.CategoryID = @CategoryID)";

            var sql = $@"SELECT
                            A.CategoryID,
                            A.MenuID,
                            A.MenuName,
                            B.Controller,
                            B.Action
                        FROM M_WebMenu A
                        LEFT OUTER JOIN M_WebMenuController B ON (A.CategoryID = B.CategoryID) AND (A.MenuID = B.MenuID)
                        WHERE (1=1)
                            AND (A.CompanyID = @CompanyID)
                            AND (A.{userRoleName} = @RoleFlag)
                            {whereString}
                        ORDER BY SortNumber ASC
                        ;";

            return sql;
        }
    }
}
