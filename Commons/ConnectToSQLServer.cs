using Dapper;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// SQLServer接続に関する関数
    /// </summary>
    public static class ConnectToSQLServer
    {
        /// <summary>
        /// SQLServer接続文字列取得(共通マスター)
        /// </summary>
        /// <returns></returns>
        public static string GetSQLServerConnectionStringForMaster()
        {
            var databaseName = "WarehouseMaster";
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            return configuration.GetSection("connectionString").GetValue<string>(databaseName);
        }

        /// <summary>
        /// SQLServer接続文字列取得
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string GetSQLServerConnectionString(string databaseName)
        {
            var connectionStringFirst = "ConnectionStringFirst";
            var connectionStringSecond = "ConnectionStringSecond";
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            return configuration.GetSection("connectionString").GetValue<string>(connectionStringFirst) + databaseName + configuration.GetSection("connectionString").GetValue<string>(connectionStringSecond);
        }

        /// <summary>
        /// 同じレコードが存在するかチェック
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>同じレコードが存在する場合はtrueを返す</returns>
        public static bool IsExistedSameRecord(string sql, string databaseName)
        {
            // 戻り値
            bool IsExisted = false;

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

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
