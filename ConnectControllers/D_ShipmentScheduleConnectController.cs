using Dapper;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 出荷指示テーブルに関する関数
    /// </summary>
    public static class D_ShipmentScheduleConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>出荷指示情報</returns>
        public static List<D_ShipmentScheduleModel> ConnectDShipmentSchedules(string sql, string databaseName)
        {
            // 戻り値
            List<D_ShipmentScheduleModel> strList = new();

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

                    strList = connection.Query<D_ShipmentScheduleModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社マスターSELECT文SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDShipmentSchedules()
        {
            var sql = $@"
                    SELECT
                       *
                    FROM 
                        D_ShipmentSchedule
                    WHERE
                        IsDeleted = 0
                ;";

            return sql;
        }
    }
}
