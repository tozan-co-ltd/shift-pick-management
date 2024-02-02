using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;

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
        /// <param name="sql">SQL</param>
        /// <returns>ユーザー情報</returns>
        public static M_CompanyModel? ConnectMCompanny(string sql)
        {
            // 戻り値
            List<M_CompanyModel> companyModels = new();

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

                    companyModels = connection.Query<M_CompanyModel>(sql).ToList();
                    // 件数をチェック
                    if(companyModels.Count != 1)
                    {
                        // エラーを作成
                        // エラーコード：E2011
                        throw new Exception();
                    }
                }
                return companyModels.FirstOrDefault();
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
        public static string CreateSQLToSelectMCompany()
        {
            var sql = $@"
                    SELECT
                       CompanyID
                       ,CompanyCode
                       ,CompanyPassword
                       ,CompanyWebPath
                       ,CompanyName
                       ,CompanyNameKana
                       ,DatabaseName
                       ,HandyApiUrl
                       ,HandyAppMinVersion
                       ,HandyAdminPassword
                       ,NotUseFlag
                    FROM 
                        M_Company
                ";

            return sql;
        }

        /// <summary>
        /// 会社URLで会社取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMCompanyByWebPath(string companyWebPath)
        {
            var sql = CreateSQLToSelectMCompany();
            sql += $@"
                    WHERE
                        CompanyWebPath = '{companyWebPath}'
                        AND NotUseFlag = 0;
                ";

            return sql;
        }
       
    }
}
