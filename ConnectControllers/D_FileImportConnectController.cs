using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using System.Reflection;

namespace mar_sumaken_web.ConnectControllers
{
    public static class D_FileImportConnectController
    {

        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>倉庫マスター情報</returns>
        public static List<D_FileImportModel> ConnectD_FileImport(string sql, string databaseName)
        {
            // 戻り値
            List<D_FileImportModel> strList = new();

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

                    strList = connection.Query<D_FileImportModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 出荷指示取込一覧取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetD_FileImport()
        {
            var sql = $@"
                        SELECT 
                            FORMAT (CreatedAt, 'yyyy/MM/dd HH:mm:ss') AS CreatedAt,
                            ImportFileName,
                            CreatedBy
                        FROM 
	                        D_FileImport
                        WHERE CreatedAt >= DATEADD(MONTH, -1, GETDATE());
            ";

            return sql;
        }
    }
}
