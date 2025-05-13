using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class AlertRecordConnectController 
    {
        /// <summary>
        /// アラート履歴情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<AlertRecordModel> ConnectTAlertRecords(string sql)
        {
            // 戻り値
            List<AlertRecordModel> strList = new();

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
                    strList = connection.Query<AlertRecordModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// アラート履歴情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <returns></returns>
        public static string CreateSQLToSelectAlertRecord(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT
                    AlertRecords.alert_record_id
                    ,AlertRecords.trip_record_id
                    ,AlertRecords.notification_id
                    ,AlertRecords.arrival_lower_load_class
                    ,AlertRecords.departure_lower_load_class
                    ,AlertRecords.created_at
                    ,AlertRecords.updated_at
                FROM t_alert_records AS AlertRecords
                INNER JOIN
                    t_trip_records AS TripRecords
                ON 
                    AlertRecords.trip_record_id = TripRecords.trip_record_id
                INNER JOIN
                    m_stations AS Stations
                ON
                    TripRecords.station_id = Stations.station_id
                WHERE
                    TripRecords.work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
            ";
            if (checkedDepos.Count != 0)
            {
                if (checkedDepos[0] != "0")
                {
                    sql += "AND (";
                    for (int i = 0; i < checkedDepos.Count; i++)
                    {
                        if (i != 0)
                        {
                            sql += $" OR ";
                        }
                        sql += $"Stations.depo_id = {checkedDepos[i]}";
                    }
                    sql += $@")";
                }
            }
            return sql;
        }

        /// <summary>
        /// アラート履歴に対応する荷量実績取得SQL
        /// </summary>
        /// <param name="alertRecords">アラート履歴</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordFromAlertRecord(List<AlertRecordModel> alertRecords)
        {
            var sql = $@"
                SELECT
                    TripRecords.trip_record_id
                    ,TripRecords.trip_name
                    ,TripRecords.trip_branch_seq
                    ,TripRecords.driver_name
                    ,Stations.name AS station_name
                    ,TripRecords.truck_number
                    ,TripRecords.identify_number
	                ,FORMAT(CONVERT(DATETIME, TripRecords.arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time
	                ,FORMAT(CONVERT(DATETIME, TripRecords.departure_scheduled_time), 'HH:mm') AS departure_scheduled_time
	                ,FORMAT(TripRecords.work_day, 'yyyy/MM/dd') AS work_day
	                ,FORMAT(TripRecords.arrived_at, 'yyyy/MM/dd HH:mm') AS arrived_at
	                ,FORMAT(TripRecords.departed_at, 'yyyy/MM/dd HH:mm') AS departed_at
	                ,TripRecords.arrival_load_class
	                ,TripRecords.departure_load_class
                FROM t_trip_records AS TripRecords
                INNER JOIN
                    m_stations AS Stations
                    ON
                    TripRecords.station_id = Stations.station_id
                WHERE
                    TripRecords.is_deleted <> 1
                AND
                    {GetAlertRecordListSQL(alertRecords)}
            ";
            return sql;
        }

        private static string GetAlertRecordListSQL(List<AlertRecordModel> alertRecords)
        {

            var sql = "";
            for(int i=0; i<alertRecords.Count; i++)
            {
                if(i != 0) sql += " OR ";

                sql += $@"TripRecords.trip_record_id = {alertRecords[i].TripRecordID}";
            }
            return sql;
        }

        /// <summary>
        /// アラート履歴DataTable取得SQL
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectAlertRecordForDataTable(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT
                    AlertRecords.alert_record_id
                    ,TripRecords.trip_name
                    ,TripRecords.trip_branch_seq
                    ,TripRecords.driver_name
                    ,Stations.name AS station_name
                    ,TripRecords.truck_number
                    ,TripRecords.identify_number
                    ,FORMAT(CONVERT(DATETIME, arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time
	                ,FORMAT(CONVERT(DATETIME, departure_scheduled_time), 'HH:mm') AS departure_scheduled_time
	                ,FORMAT(work_day, 'yyyy/MM/dd') AS work_day
	                ,FORMAT(arrived_at, 'yyyy/MM/dd HH:mm') AS arrived_at
	                ,FORMAT(departed_at, 'yyyy/MM/dd HH:mm') AS departed_at
                    ,TripRecords.arrival_load_class
                    ,AlertRecords.arrival_lower_load_class
                    ,TripRecords.departure_load_class
                    ,AlertRecords.departure_lower_load_class
                    ,TripRecords.arrival_load_img_path
                    ,TripRecords.departure_load_img_path
                FROM t_alert_records AS AlertRecords
                INNER JOIN
                    t_trip_records AS TripRecords
                ON 
                    AlertRecords.trip_record_id = TripRecords.trip_record_id
                INNER JOIN
                    m_stations AS Stations
                ON
                    TripRecords.station_id = Stations.station_id
                WHERE
                    TripRecords.work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
            ";
            return sql;
        }
    }
}
