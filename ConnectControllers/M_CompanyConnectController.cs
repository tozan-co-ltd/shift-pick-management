using Dapper;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Commons
{
    /// <summary>
    /// 会社マスターに関する関数
    /// </summary>
    public static class M_CompanyConnectController
    {
        /// <summary>
        /// 会社情報取得
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_CompanyModel> ConnectMCompanys(string sql, string databaseName)
        {
            // 戻り値
            List<M_CompanyModel> strList = new();

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

                    strList = connection.Query<M_CompanyModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>インサート数</returns>
        public static int InsertMCompany(M_CompanyModel model, LoginUserModel loginUser)
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
                    // 会社マスター登録SQL作成
                    string companyRegisterSql = CreateSQLToInsertMCompany(model, sysDate, loginUser.UserName);
                    // 会社マスター登録
                    var insertedCount = connection.Execute(companyRegisterSql);

                    return insertedCount;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 会社情報更新
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>インサート数</returns>
        public static int UpdateMCompany(M_CompanyModel model, LoginUserModel loginUser)
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
                    // 会社マスター更新SQL作成
                    string companyRegisterSql = CreateSQLToUpdateMCompany(model, sysDate, loginUser.UserName);
                    // 会社マスター更新
                    var editedCount = connection.Execute(companyRegisterSql);

                    return editedCount;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 会社マスター削除
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMCompany(int companyId, LoginUserModel loginUser)
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
                    string sql = CreateSQLToDeleteMCompany(companyId, sysDate, loginUser.UserName);
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
        /// 会社コードで会社IDを取得
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static int GetCompanyIdByCompanyCode(string? companyCode, string databaseName)
        {
            var result = -1;
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    // SQL作成
                    string sql = $@"
                        SELECT TOP 1 CompanyID FROM M_Company WHERE CompanyCode = {companyCode}
                    ";
                    // 会社コード取得
                    var companyId = connection.ExecuteScalar<int>(sql);
                    if (companyId > 0)
                    {
                        return companyId;
                    }

                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 会社コードで会社を取得
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static M_CompanyModel GetMCompanyByCompanyCode(string? companyCode, string databaseName)
        {
            var result = -1;
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    // SQL作成
                    string sql = $@"
                        SELECT * FROM M_Company WHERE CompanyCode = {companyCode}
                    ";
                    // 会社コード取得
                    var mCompany = connection.Query<M_CompanyModel>(sql);
                    if (mCompany != null && mCompany.Count() > 0)
                    {
                        return mCompany.ToList().FirstOrDefault();
                    }

                    return null;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 会社マスターSELECT文SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMCompanys()
        {
            var sql = $@"
                    SELECT
                       company.CompanyID
                       ,company.CompanyCode
                       ,company.CompanyKubun
                       ,CASE 
                            WHEN company.CompanyKubun = 1 THEN '得意先'
                            WHEN company.CompanyKubun = 2 THEN '仕入先'
                            WHEN company.CompanyKubun = 3 THEN '納入先'
                            ELSE ''
                        END AS CompanyKubunName
                       ,company.CompanyName
                       ,company.ClientName
                       ,company.IsDeleted
                       ,company.CreatedAt
                       ,company.CreatedBy
                       ,company.UpdatedAt
                       ,company.UpdatedBy
                    FROM 
                        M_Company AS company
                    WHERE
                        company.IsDeleted = 0
                    ORDER BY company.CompanyID ASC
                ;";

            return sql;
        }

        /// <summary>
        /// 重複会社情報取得SQL作成
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMCompany(int companyCode)
        {
            var sql = $@"
                    SELECT
                        COUNT(*)                      
                    FROM 
                        M_Company
                    WHERE
                        CompanyCode = {companyCode}
                        AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なるIDで重複会社情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMCompany(M_CompanyModel model)
        {
            var sql = $@"
                    SELECT
                        COUNT(*)                      
                    FROM 
                        M_Company
                    WHERE
                        CompanyCode = {model.CompanyCode}
                        AND CompanyID <> {model.CompanyID}
                        AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 会社IDで会社情報を取得
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectByCompanyId(int companyId)
        {
            var sql = $@"
                    SELECT
                        *               
                    FROM 
                        M_Company
                    WHERE
                        CompanyID = {companyId}
                        AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 会社マスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMCompany(M_CompanyModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO M_Company(
                    CompanyCode, 
                    CompanyKubun, 
                    CompanyName, 
                    ClientName, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                VALUES (
                    '{model.CompanyCode}',
                    '{model.CompanyKubun}',
                    '{model.CompanyName}',
                    '{model.ClientName}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 会社マスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMCompany(M_CompanyModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Company
                SET 
                    CompanyCode = '{model.CompanyCode}',
                    CompanyKubun = '{model.CompanyKubun}',
                    CompanyName = '{model.CompanyName}',
                    ClientName = '{model.ClientName}',
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    CompanyID = {model.CompanyID}
                    and IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 会社マスター削除SQL作成
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMCompany(int companyId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Company
                SET 
                    IsDeleted = 1,
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE 
                    CompanyID = {companyId}
            ;";
            return sql;
        }
    }
}
