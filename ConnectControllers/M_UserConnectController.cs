using Dapper;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using static mar_sumaken_web.Models.M_UserModel;

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
        /// ユーザーマスターリストの詳細を取得する
        /// </summary>
        /// <param name="userList"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static List<M_UserModel.M_User> GetUserListDetail(List<M_User> userList, string databaseName)
        {
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    if (userList.Count > 0)
                    {
                        foreach (M_User user in userList)
                        {
                            // 倉庫マスター情報取得
                            var userDepoSql = CreateSQLToGetUserDepoList(user.UserID);
                            List<M_DepoModel> depoList = connection.Query<M_DepoModel>(userDepoSql).ToList();
                            if(depoList.Count > 0)
                            {
                                user.M_DepoList = depoList;
                            }

                            // ハンディメニューマスター情報取得
                            var userHandyMenuSql = CreateSQLToGetUserHandyMenuList(user.UserID);
                            List<M_HandyMenuModel> handyMenuList = connection.Query<M_HandyMenuModel>(userHandyMenuSql).ToList();
                            if (handyMenuList.Count > 0)
                            {
                                user.M_HandyMenuList = handyMenuList;
                            }
                        }
                    }
                }
                return userList;
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
                        ,CASE 
                            WHEN AuthorizedKubun = 1 THEN '管理者'
                            WHEN AuthorizedKubun = 2 THEN '作業者'
                            WHEN AuthorizedKubun = 3 THEN '作業者(解除要)'
                            ELSE''
                         END AS AuthorizedKubunName
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

        /// <summary>
        /// ユーザーマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetMUsers()
        {
            var sql = CreateSQLToSelectMUsers();
            sql += $@"
                    WHERE
                        NotUseFlag = 0
                ";

            return sql;
        }

        /// <summary>
        /// 倉庫マスター情報取得SQL作成
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string CreateSQLToGetUserDepoList(int userId)
        {
            var sql = $@"
                        SELECT 
	                        userDepo.DepoID,
	                        m_depo.DepoCode,
	                        m_depo.DepoName
                        FROM 
	                        R_UserDepo AS userDepo
                        INNER JOIN M_Depo AS m_depo 
                            ON userDepo.DepoID = m_depo.DepoID
                        WHERE 
	                        userDepo.UserID = {userId}
                            AND m_depo.NotUseFlag = 0
                    ";
            return sql;
        }

        /// <summary>
        /// ハンディメニューマスター情報取得SQL作成
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string CreateSQLToGetUserHandyMenuList(int userId)
        {
            var sql = $@"
                         SELECT 
	                        userMenu.HandyMenuID,
	                        menu.HandyMenuName
                         FROM 
	                        R_UserHandyMenu AS userMenu
                         INNER JOIN M_HandyMenu AS menu 
                            ON userMenu.HandyMenuID = menu.HandyMenuID
                         WHERE 
	                        userMenu.UserID = {userId}
                            AND menu.NotUseFlag = 0
                    ";
            return sql;
        }

    }
}
