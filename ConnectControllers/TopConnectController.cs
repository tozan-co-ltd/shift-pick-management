using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using System.Data;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class TopConnectController {

        /// <summary>
        /// 便情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<TopModel> ConnectTops(string sql)
        {
            // 戻り値
            List<TopModel> strList = new();

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
                    strList = connection.Query<TopModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 最新のステーション状況取得SQL
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectLatestStationStatus()
        {
            var sql = $@"
                SELECT 
	                station_id,
	                load_class,
	                image_base64,
	                updated_at
                FROM t_tmp_station_status
            ";
            return sql;
        }

        /// <summary>
        /// ステーション毎のトラック有無取得SQL
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectIsExistTrucksPerStationID()
        {
            var sql = $@"
                SELECT truck_sensor_records.station_id
                       ,truck_exist
                FROM t_truck_sensor_records truck_sensor_records
                JOIN (
	                SELECT station_id, MAX(created_at) AS latest_create
	                FROM t_truck_sensor_records
	                GROUP BY station_id
                ) latest_detect_recprds
                ON truck_sensor_records.station_id = latest_detect_recprds.station_id 
                AND truck_sensor_records.created_at = latest_detect_recprds.latest_create
            ";
            return sql;
        }
    }
}
