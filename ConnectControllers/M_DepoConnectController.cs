using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 倉庫マスターに関する関数
    /// </summary>
    public static class M_DepoConnectController
    {
        /// <summary>
        /// 倉庫情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_DepoModel> ConnectMDepos(string sql, string databaseName)
        {
            // 戻り値
            List<M_DepoModel> strList = new();

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

                    strList = connection.Query<M_DepoModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 倉庫マスター情報取得
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <returns>倉庫マスター情報</returns>
        public static List<M_DepoModel> GetMDepoList(string databaseName)
        {
            try
            {
                // SQL作成
                var sql = CreateSQLToGetMDepoList();
                // DB接続
                List<M_DepoModel> userList = ConnectMDepos(sql, databaseName);
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 倉庫マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetMDepoList()
        {
            var sql = $@"
                        SELECT 
	                        DepoID
                            ,DepoCode
                            ,DepoName
                            ,IsDeleted
                            ,CreatedAt
                            ,CreatedBy
                            ,UpdatedAt
                            ,UpdatedBy
                        FROM 
	                        M_Depo
                        WHERE 
                            IsDeleted = 0
            ";
            return sql;
        }
    }
}
