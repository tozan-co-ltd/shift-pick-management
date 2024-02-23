using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// warehouse_0_masterの会社マスターに関する関数
    /// </summary>
    public static class Warehouse_M_CompanyConnectController
    {
        /// <summary>
        /// 会社情報取得
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public static Warehouse_M_CompanyModel? ConnectMCompanny(string sql)
        {
            // 戻り値
            List<Warehouse_M_CompanyModel> companyModels = new();

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

                    companyModels = connection.Query<Warehouse_M_CompanyModel>(sql).ToList();
                    // 件数チェック
                    if(companyModels.Count != 1)
                    {
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
        /// 会社URLで会社取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMCompanyByWebPath(string companyWebPath)
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
                WHERE
                    CompanyWebPath = '{companyWebPath}'
                    AND NotUseFlag = 0;
            ";

            return sql;
        }
    }
}
