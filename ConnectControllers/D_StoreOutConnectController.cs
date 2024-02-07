using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using System.Reflection;

namespace mar_sumaken_web.ConnectControllers
{
    public static class D_StoreOutConnectController
    {

        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>倉庫マスター情報</returns>
        public static List<D_StoreOutModel.D_StoreOut> ConnectD_StoreOut(string sql, string databaseName)
        {
            // 戻り値
            List<D_StoreOutModel.D_StoreOut> strList = new();

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

                    strList = connection.Query<D_StoreOutModel.D_StoreOut>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 出庫情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetD_StoreOutList(D_StoreOutModel.D_StoreOut model)
        {
            string dateSearchStart = model.DateSearchStart + " " + "00:00:00.000";
            string dateSearchEnd = model.DateSearchEnd + " " + "23:59:59.999";

            var sql = $@"
                        SELECT 
                            StoreOutID,
                            StoreOutDate
                        FROM 
	                        D_StoreOut
                        WHERE StoreOutDate >= CONVERT(datetime, '{dateSearchStart}') 
                        AND StoreOutDate <= CONVERT(datetime, '{@dateSearchEnd}');
            ";

            return sql;
        }
    }
}
