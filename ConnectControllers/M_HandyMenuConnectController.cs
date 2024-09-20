using Dapper;
using  ai_truck_load_measurement.Commons;
using  ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace  ai_truck_load_measurement.ConnectControllers
{
    /// <summary>
    /// ハンディメニューマスターに関する関数
    /// </summary>
    public class M_HandyMenuConnectController
    {
        /// <summary>
        /// ハンディメニューマスター情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_HandyMenuModel> ConnectMHandyMenus(string sql, string databaseName)
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
        /// ハンディメニューマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMHandyMenuList()
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
