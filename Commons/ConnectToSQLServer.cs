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
    }
}