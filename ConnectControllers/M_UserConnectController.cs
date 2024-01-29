using Dapper;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using mar_sumaken_web.Models;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// ユーザーマスターに関する関数
    /// </summary>
    public static class M_UserConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>ユーザー情報</returns>
        public static List<M_UserModel.M_User> ConnectMUsers(string sql, string databaseName)
        {
            // 戻り値
            List<M_UserModel.M_User> strList = new();

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

                    strList = connection.Query<M_UserModel.M_User>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーマスターSELECT文SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMUsers()
        {
            var sql = $@"
                    SELECT
                        UserID                                
                        ,LoginID                              
                        ,UserName
                        ,DepoID
                        ,AuthorizedKubun
                        ,FORMAT (CreatedAt, 'yyyy/MM/dd ')     AS CreatedAt
                        ,CreatedBy
                        ,FORMAT (UpdatedAt, 'yyyy/MM/dd ')     AS UpdatedAt
                        ,UpdatedBy                            
                    FROM 
                        M_User
                ";

            return sql;
        }

        /// <summary>
        /// 一致するユーザー取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetMUsersByCondition(string userId, string loginId, string isDeleted)
        {
            var sql = CreateSQLToSelectMUsers();
            sql += $@"
                    WHERE
                        user_id         = '{@userId}'
                        AND login_id    = '{@loginId}'
                        AND is_deleted  = '{@isDeleted}'
                ";

            return sql;
        }
       
    }
}
