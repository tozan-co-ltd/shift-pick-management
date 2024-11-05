using Dapper;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Commons
{
    /// <summary>
    /// SQLServer接続に関する関数
    /// </summary>
    public static class ConnectToSQLServer
    {
        /// <summary>
        /// SQLServer接続文字列取得
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string GetSQLServerConnectionString()
        {
            var databaseName = "AITruckLoadMeasurementMaster";
#if DEBUG
            databaseName = "AITruckLoadMeasurementMasterTest";
#endif
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            return configuration.GetSection("connectionString").GetValue<string>(databaseName);
        }

        /// <summary>
        /// 同じレコードが存在するかチェック
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="">データベース名</param>
        /// <returns>同じレコードが存在する場合はtrueを返す</returns>
        public static bool IsExistedSameRecord(string sql)
        {
            // 戻り値
            bool IsExisted = false;

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                    int result = Convert.ToInt32(connection.ExecuteScalar(sql));

                    if (result > 0)
                    {
                        IsExisted = true;
                    }
                }
                return IsExisted;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
