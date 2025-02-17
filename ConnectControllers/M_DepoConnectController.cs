using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_DepoConnectController
    {
        /// <summary>
        /// デポ情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_DepoModel> ConnectMDepos(string sql)
        {
            // 戻り値
            List<M_DepoModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    strList = connection.Query<M_DepoModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーのAD名からデポ情報を取得するSQL生成
        /// </summary>
        /// <param name="ADName">AD名</param>
        /// <returns></returns>
        public static string CreateSQLToSelectDepoFromADName(string ADName)
        {
            var sql = $@"
                SELECT
                    Depos.depo_id,
                    name
	                ,gateway
                    ,Depos.created_at
                    ,Depos.created_by
                    ,Depos.updated_at
                    ,Depos.updated_by
                FROM
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE Users.ad_name = '{ADName}'
            ";
            return sql;
        }
    }
}
