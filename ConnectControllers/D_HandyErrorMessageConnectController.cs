using Dapper;
using System.Data.SqlClient;
using  ai_truck_load_measurement.Models;
using  ai_truck_load_measurement.Commons;

namespace  ai_truck_load_measurement.ConnectControllers
{
    /// <summary>
    /// ハンディエラーメッセージ実績テーブルに関する関数
    /// </summary>
    public static class D_HandyErrorMessageConnectController
    {
        /// <summary>
        /// ハンディエラーメッセージ実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_HandyErrorMessageModel> ConnectDHandyErrorMessages(string sql, string databaseName)
        {
            // 戻り値
            List<D_HandyErrorMessageModel> strList = new();

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

                    strList = connection.Query<D_HandyErrorMessageModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ハンディエラーメッセージ実績一覧取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDHandyErrorMessages()
        {
            var sql = $@"
                SELECT 
                    HandyErrorMessageID,
                    DepoName,
                    HandyMenuName,
                    ErrorMessage,
                    FirstScanedString,
                    SecondScanedString,
                    FORMAT(handyError.CreatedAt, 'yyyy/MM/dd HH:mm') AS CreatedAt,
                    handyError.CreatedBy,
                    UnlockedBy
                FROM D_HandyErrorMessage AS handyError
                INNER JOIN M_Depo AS depo
                    ON handyError.DepoID = depo.DepoID
                INNER JOIN M_HandyMenu AS handyMenu
                    ON handyError.HandyMenuID = handyMenu.HandyMenuID
                WHERE
                    handyError.CreatedAt >= DATEADD(WW, -1, GETDATE());
            ";

            return sql;
        }
    }
}
