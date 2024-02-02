using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    public class M_HandyMenuConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>ハンディメニューマスター情報</returns>
        public static List<M_HandyMenuModel> ConnectMHandyMenu(string sql, string databaseName)
        {
            // 戻り値
            List<M_HandyMenuModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    strList = connection.Query<M_HandyMenuModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ハンディメニューマスター情報取得
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <returns>ハンディメニューマスター情報</returns>
        public static List<M_HandyMenuModel> GetMHandyMenuList(string databaseName)
        {
            try
            {
                // SQL作成
                var sql = CreateSQLToGetMHandyMenuList();
                // DB接続
                List<M_HandyMenuModel> userList = ConnectMHandyMenu(sql, databaseName);
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ハンディメニューマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetMHandyMenuList()
        {
            var sql = $@"
                        SELECT 
	                       HandyMenuID
                           ,SortNumber
                           ,HandyMenuName
                           ,IsDeleted
                           ,CreatedAt
                           ,CreatedBy
                           ,UpdatedAt
                           ,UpdatedBy
                        FROM 
	                        M_HandyMenu
                        WHERE 
                            IsDeleted = 0
            ";
            return sql;
        }
    }
}
