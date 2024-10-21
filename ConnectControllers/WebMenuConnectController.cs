using Dapper;
using ai_truck_load_measurement.Models;
using System.ComponentModel.Design;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Commons
{
    /// <summary>
    /// WEBメニューに関する関数
    /// </summary>
    public static class WebMenuConnectController
    {
        /// <summary>
        /// WEBメニューカテゴリー情報取得
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public static List<M_WebMenuCategory> ConnectMWebMenuCategory(string sql)
        {
            // 戻り値
            List<M_WebMenuCategory> strList = new();

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

                    var param = new{};

                    strList = connection.Query<M_WebMenuCategory>(sql, param).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// WEBメニューカテゴリー取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMWebMenuCategory()
        {
            var sql = $@"SELECT
                            CategoryID,
                            CategoryName
                        FROM M_WebMenuCategory
                        ORDER BY CategoryCode ASC;
            ;";

            return sql;
        }

        /// <summary>
        /// WEBメニュー情報取得
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="companyID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public static List<M_WebMenu> ConnectMWebMenu(string sql, int companyID, int categoryID)
        {
            // 戻り値
            List<M_WebMenu> strList = new();

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
                        CompanyID = companyID,
                        RoleFlag = 1,
                        CategoryID = categoryID
                    };

                    strList = connection.Query<M_WebMenu>(sql, param).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// WEBメニュー取得SQL作成
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="userRoleName"></param>
        /// <param name="categoryID"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMWebMenu(int companyID, string userRoleName, int categoryID)
        {
            var sql = $@"SELECT
                            A.CategoryID,
                            A.MenuID,
                            A.MenuName,
                            B.Controller,
                            B.Action
                        FROM M_WebMenu A
                        LEFT OUTER JOIN M_WebMenuController B 
                            ON (A.CategoryID = B.CategoryID)
                            AND (A.MenuID = B.MenuID)
                        WHERE (1=1)
                            AND (A.CompanyID = {companyID})
                            AND (A.{userRoleName} = @RoleFlag)";

            if(categoryID != 0)
            {
                sql += $@" AND (A.CategoryID = @CategoryID)";
            }

            sql += $@" ORDER BY SortNumber ASC;";

            return sql;
        }
    }
}
