using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadOutputConnectController
    {
        /// <summary>
        /// 便実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<LoadOutputModel> ConnectTTripRecords(string sql)
        {
            // 戻り値
            List<LoadOutputModel> strList = new();

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
                    strList = connection.Query<LoadOutputModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// 指定した期間の便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecordFromPeriod(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDeference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT DISTINCT
                    TripRecords.trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
                    Depos.name AS depo_name,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                ";
            if (isOnlyHasAmountDeference)
            {
                sql += $@"
                FROM t_annotation_loads
                INNER JOIN t_trip_records AS TripRecords
                ON t_annotation_loads.trip_record_id = TripRecords.trip_record_id
                ";
            }
            else
            {
                sql += $@"
                FROM t_trip_records AS TripRecords";
            }
            sql += $@"
                JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
            ";
            if (hasTripName)
            {
                sql += $@"
                AND NOT trip_name IS NULL ";
            }
            if (hasIdentifyNumber)
            {
                sql += $@"
                AND NOT identify_number IS NULL ";
            }
            sql += SQLOfCheckedDepos(checkedDepos);
            return sql;
        }

        /// <summary>
        /// 画像出力用の便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecordForImage(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDeference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT
                    TripRecords.trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                TripRecords.driver_name,
	                TripRecords.station_id,
	                truck_number,
	                identify_number,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                ";
            if (isOnlyHasAmountDeference)
            {
                sql += $@"
                    ,arrival_departure_class
                FROM t_annotation_loads 
                INNER JOIN t_trip_records AS TripRecords
                ON t_annotation_loads.trip_record_id = TripRecords.trip_record_id
                ";
            }
            else
            {
                sql += $@"
                FROM t_trip_records AS TripRecords";
            }
            sql += $@"
                JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'";
            if (hasTripName)
            {
                sql += $@"
                AND NOT trip_name IS NULL";
            }
            if (hasIdentifyNumber)
            {
                sql += $@"
                AND NOT identify_number IS NULL";
            }
            sql += SQLOfCheckedDepos(checkedDepos);
            sql += $@"
            ORDER BY arrived_at";
            return sql;
        }

        /// <summary>
        /// データベース用便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordForDataTable(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDeference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepo)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
                    Depos.name AS depo_name,
	                FORMAT(CONVERT(DATETIME, arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time,
	                FORMAT(CONVERT(DATETIME, departure_scheduled_time), 'HH:mm') AS departure_scheduled_time,
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day,
	                FORMAT(arrived_at, 'yyyy/MM/dd HH:mm'),
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm'),
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                ";
            if (isOnlyHasAmountDeference)
            {
                sql += $@"
                FROM t_annotation_loads
                INNER JOIN t_trip_records AS TripRecords
                ON t_annotation_loads.trip_record_id = TripRecords.trip_record_id
                ";
            }
            else
            {
                sql += $@"
                FROM t_trip_records AS TripRecords";
            }

            sql += $@"
                JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'";
            if (hasTripName)
            {
                sql += $@"
                AND NOT trip_name IS NULL";
            }
            if (hasIdentifyNumber)
            {
                sql += $@"
                AND NOT identify_number IS NULL";
            }
            sql += SQLOfCheckedDepos(checkedDepo);
            sql += $@"
            ORDER BY trip_name, work_day, trip_branch_seq
            ";
            return sql;
        }

        private static string SQLOfCheckedDepos(List<string> checkedDepos)
        {
            var sql = "";
            if (checkedDepos.Count > 0)
            {
                sql += "AND Depos.depo_id IN (";
                for (int i = 0; i < checkedDepos.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += $", ";
                    }
                    sql += $"'{checkedDepos[i]}'";
                }
                sql += ")";
            }
            return sql;
        }
    }
}
