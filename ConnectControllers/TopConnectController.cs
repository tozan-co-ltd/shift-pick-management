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
        public static List<ViewCardModel> ConnectTops(string sql)
        {
            // 戻り値
            List<ViewCardModel> strList = new();

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
                    strList = connection.Query<ViewCardModel>(sql).ToList();
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
        public static string CreateSQLToSelectLatestStationStatus(int depoID)
        {
            var sql = $@"
                SELECT 
                    detect_records.station_id,
                    load_class,
                    ip_adress,
                    station_seq
                 FROM t_load_detect_records AS detect_records
                 JOIN (
                    SELECT station_id, MAX(created_at) AS latest_create
                    FROM t_load_detect_records
                    GROUP BY station_id
                ) latest_detect_records
                ON detect_records.station_id = latest_detect_records.station_id 
                AND detect_records.created_at = latest_detect_records.latest_create
                JOIN m_devices
                ON detect_records.station_id = m_devices.station_id
                JOIN m_stations
                 ON detect_records.station_id = m_stations.station_id
                 WHERE depo_id = {depoID}
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
                ) latest_detect_records
                ON truck_sensor_records.station_id = latest_detect_records.station_id 
                AND truck_sensor_records.created_at = latest_detect_records.latest_create
            ";
            return sql;
        }
    }
}
