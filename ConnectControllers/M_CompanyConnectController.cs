using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 会社に関する関数
    /// </summary>
    public static class M_CompanyConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>ユーザー情報</returns>
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
        /// 会社SELECT文SQL作成
        /// </summary>
        /// <returns>SQL</returns>
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
        /// 会社マスター削除
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>更新件数</returns>
        public static int DeleteMCompany(int companyId, string databaseName)
        {
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
                    // 会社マスター削除SQL作成
                    string deleteSql = CreateSQLToDeleteMCompany(companyId);
                    // 会社マスター削除
                    int deleteAffectedRows = connection.Execute(deleteSql);
                    // 更新件数が0の場合はエラーとする
                    if (deleteAffectedRows == 0)
                    {
                        // エラーコード：E2011
                        throw new Exception();
                    }

                    return deleteAffectedRows;
                }
                catch (Exception ex)
                {
                    // エラーコード：E2011
                    throw ex;
                }
            }
        }

        // <summary>
        /// 会社マスター削除SQL作成
        /// </summary>
        /// <param name="companyId">会社ID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMCompany(int companyId)
        {
            var sql = $@"
                UPDATE M_Company
                SET IsDeleted = 1
                WHERE CompanyID = {companyId}
            ;";
            return sql;
        }
    }
}
