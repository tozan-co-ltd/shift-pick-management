using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using System.Reflection;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 出庫実績テーブルに関する関数
    /// </summary>
    public static class D_StoreOutConnectController
    {
        /// <summary>
        /// 出庫実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_StoreOutModel> ConnectDStoreOuts(string sql, string databaseName)
        {
            // 戻り値
            List<D_StoreOutModel> strList = new();

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

                    strList = connection.Query<D_StoreOutModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 出庫実績情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDStoreOuts(D_StoreOutModel model)
        {
            string dateSearchStart = model.SearchStartDate + " " + "00:00:00.000";
            string dateSearchEnd = model.SearchEndDate + " " + "23:59:59.999";

            var sql = $@"
                        SELECT 
                            *
                        FROM 
	                        D_StoreOut
                        WHERE StoreOutDate >= CONVERT(datetime, '{dateSearchStart}') 
                        AND StoreOutDate <= CONVERT(datetime, '{@dateSearchEnd}');
            ";

            return sql;
        }
    }
}
