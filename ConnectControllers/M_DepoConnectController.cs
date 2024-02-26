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
        /// 倉庫情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMDepo(M_DepoModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToInsertMDepo(model, sysDate, loginUser.UserName);
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
        /// 倉庫情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int UpdateMDepo(M_DepoModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToUpdateMDepo(model, sysDate, loginUser.UserName);
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
        /// 倉庫情報削除
        /// </summary>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMDepo(int depoId, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToDeleteMDepo(depoId, sysDate, loginUser.UserName);
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
        /// 倉庫マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMDepos()
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

        /// <summary>
        /// 重複倉庫情報取得SQL作成
        /// </summary>
        /// <param name="depoCode">倉庫コード</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMDepo(int depoCode)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_Depo
                WHERE
                    DepoCode = {depoCode}
                    AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なるIDで重複倉庫情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMDepo(M_DepoModel model)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_Depo
                WHERE
                    DepoCode = {model.DepoCode}
                    AND DepoID <> {model.DepoID}
                    AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 倉庫マスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMDepo(M_DepoModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO M_Depo(
                    DepoCode, 
                    DepoName, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                VALUES (
                    '{model.DepoCode}',
                    '{model.DepoName}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 倉庫マスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMDepo(M_DepoModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Depo
                SET 
                    DepoCode = '{model.DepoCode}',
                    DepoName = '{model.DepoName}',
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    DepoID = {model.DepoID}
                    and IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 倉庫マスター削除SQL作成
        /// </summary>
        /// <param name="depoId"></param>
        /// <param name="updatedAt"></param>
        /// <param name="updatedBy"></param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMDepo(int depoId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Depo
                SET 
                    IsDeleted = 1,
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE 
                    DepoID = {depoId}
            ;";
            return sql;
        }
    }
}
