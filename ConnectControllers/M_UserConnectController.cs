using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_UserConnectController 
    {
        /// <summary>
        /// ユーザー情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_UserModel> ConnectMUsers(string sql)
        {
            // 戻り値
            List<M_UserModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {

                    connection.ConnectionString = connectionString;
                    connection.Open();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    strList = connection.Query<M_UserModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザー情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMUser(M_UserModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToInsertMUser(model, sysDate, loginUser.UserName);
                    var insertedCount = connection.Execute(sql);
                    return insertedCount;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// ユーザー情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int UpdateMUser(M_UserModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToUpdateMUser(model, sysDate, loginUser.UserName);
                    var count = connection.Execute(sql);

                    return count;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// ユーザー情報削除
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMUser(int userId, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToDeleteMUser(userId, sysDate, loginUser.UserName);
                    var count = connection.Execute(sql);

                    return count;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// ユーザー情報をデータテーブルとして取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static DataTable ConnectMUsersToDataTable(string sql)
        {
            // 戻り値
            DataTable dataTable = new DataTable();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザー情報取得用SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMUsers()
        {
            var sql = $@"
                SELECT 
                   Users.user_id,
                   Users.ad_name,
                   Users.depo_id,
                   Depos.name AS depo_name,
                   Users.authorized_kubun,
                   Users.created_at,
                   Users.created_by,
                   Users.updated_at,
                   Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// メール受け取りユーザー情報取得用SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMUsersIsRequiredMail()
        {
            var sql = $@"
                SELECT 
                   Users.user_id,
                   Users.ad_name,
                   Users.depo_id,
                   Depos.name AS depo_name,
                   Users.authorized_kubun,
                   Users.created_at,
                   Users.created_by,
                   Users.updated_at,
                   Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
                AND
                    Users.is_required_mail = 1
            ";
            return sql;
        }

        /// <summary>
        /// DataTable用のユーザーマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMUsersForDataTable()
        {
            var sql = $@"
                SELECT 
                    Users.user_id,
                    Users.ad_name,
                    Depos.name AS depo_name,
                    Users.authorized_kubun,
                    Users.is_deleted,
                    FORMAT (Users.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    Users.created_by,
                    FORMAT (Users.updated_at, 'yyyy/MM/dd HH:mm:ss'),
                    Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMUser(M_UserModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO m_users(
                    ad_name, 
                    depo_id,
                    authorized_kubun,
                    created_at,
                    created_by,
                    updated_at,
                    updated_by
                )
                VALUES (
                    '{model.ADName}',
                    '{model.DepoID}',
                    '{model.AuthorizedKubun}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }


        /// <summary>
        /// ユーザーマスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMUser(M_UserModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_users
                SET 
                    ad_name = '{model.ADName}',
                    depo_id = '{model.DepoID}',
                    authorized_kubun = '{model.AuthorizedKubun}',
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE
                    user_id = {model.UserID}
                    and is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター削除SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMUser(int userId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_users
                SET 
                    is_deleted = 1,
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE 
                    user_id = {userId}
            ;";
            return sql;
        }

        /// <summary>
        /// 異なるユーザーIDで重複AD名情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateADName(M_UserModel model)
        {
            if(string.IsNullOrEmpty(model.UserID.ToString()))
                model.UserID = 0;

            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_users
                WHERE
                    ad_name = '{model.ADName}'
                    AND user_id <> {model.UserID}
                    and is_deleted = 0
            ";

            return sql;
        }
    }
}
