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
	                TripRecords.trip_branch_seq,
                    TripBranchNumbers.tag,
	                driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
                    Depos.name AS depo_name,
	                CONVERT(DATETIME, TripRecords.arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, TripRecords.departure_scheduled_time) AS departure_scheduled_time,
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
                INNER JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                INNER JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                LEFT OUTER JOIN
                m_trip_branch_numbers AS TripBranchNumbers
                ON
                TripRecords.trip_branch_number_id = TripBranchNumbers.trip_branch_number_id
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
            sql += LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos);
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
                INNER JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                INNER JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'";
            if (hasTripName)
            {
                sql += $@"
                AND NOT trip_name IS NULL
                ";
            }
            if (hasIdentifyNumber)
            {
                sql += $@"
                AND NOT identify_number IS NULL
                ";
            }
            sql += LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos);
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
                    TripBranchNumbers.tag,
	                identify_number,
	                FORMAT(arrived_at, 'yyyy/MM/dd HH:mm') AS arrived_at,
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm') AS departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path,
	                TripRecords.driver_name,
	                Stations.name AS station_name,
	                truck_number,
                    Depos.name AS depo_name,
	                FORMAT(CONVERT(DATETIME, TripRecords.arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time,
	                FORMAT(CONVERT(DATETIME, TripRecords.departure_scheduled_time), 'HH:mm') AS departure_scheduled_time,
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day
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
                INNER JOIN
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                INNER JOIN
                m_depos AS Depos
                ON
                Stations.depo_id = Depos.depo_id
                LEFT OUTER JOIN
                m_trip_branch_numbers AS TripBranchNumbers
                ON
                TripRecords.trip_branch_number_id = TripBranchNumbers.trip_branch_number_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ";
            if (hasTripName)
            {
                sql += $@"
                AND NOT trip_name IS NULL
                ";
            }
            if (hasIdentifyNumber)
            {
                sql += $@"
                AND NOT identify_number IS NULL
                ";
            }
            sql += LoadRecordConnectController.SQLOfCheckedDepos(checkedDepo);
            sql += $@"
            ORDER BY trip_name, work_day, trip_branch_seq
            ";
            return sql;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepo"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectDepartedAtIsNull(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepo)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT
	                a.work_day
	                ,a.depo_id
	                ,b.null_count1
	                ,c.null_count2
                FROM 
	                (SELECT 
		                work_day
		                ,depo_id
	                FROM t_trip_records AS d
	                INNER JOIN m_stations AS e
	                ON d.station_id = e.station_id
	                GROUP BY work_day, depo_id
	                )AS a
                LEFT OUTER JOIN
	                (SELECT
		                work_day
		                ,depo_id
		                ,COUNT(trip_record_id) AS null_count1
	                FROM t_trip_records AS f
	                INNER JOIN m_stations AS g
	                ON f.station_id = g.station_id
	                WHERE departed_at IS NULL
	                AND f.trip_id IS NOT NULL
	                GROUP BY work_day, depo_id
	                ) AS b
                ON a.work_day = b.work_day
                AND a.depo_id = b.depo_id
                LEFT OUTER JOIN
	                (SELECT
		                work_day
		                ,depo_id
		                ,COUNT(trip_record_id) AS null_count2
	                FROM t_trip_records AS h
	                INNER JOIN m_stations AS i
	                ON h.station_id = i.station_id
	                WHERE departed_at IS NULL
	                AND h.trip_id IS NULL
	                GROUP BY work_day, depo_id
	                ) AS c
                ON a.work_day = c.work_day
                AND a.depo_id = c.depo_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
            ";
            sql += LoadRecordConnectController.SQLOfCheckedDepos(checkedDepo);
            sql += $@"
                ORDER BY work_day
            ";
            return sql;
        }
    }

}
