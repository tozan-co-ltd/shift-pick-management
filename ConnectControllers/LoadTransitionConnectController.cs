using Dapper;
using shift_pick_management.Commons;
using shift_pick_management.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace shift_pick_management.ConnectControllers
{
    public class LoadTransitionConnectController 
    {
        /// <summary>
        /// 検索条件から荷量クラスを取得するSQL
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <param name="tripBranchSeq">便枝番</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditions(string tripName, int tripBranchSeq, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var sql = $@"
                SELECT 
                    work_day,
	                arrival_load_class,
	                departure_load_class
                FROM t_trip_records
                WHERE trip_name = '{tripName}'
                AND trip_branch_seq = '{tripBranchSeq}'
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
            ";
            return sql;
        }

        /// <summary>
        /// 検索条件から荷量クラスを取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditionsForTable(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var selectedTrips= LoadRecordConnectController.SelectedTripsSQL(models);
            var sql = $@"
                SELECT 
                    trip_record_id,
	                trip_name,
	                trip_branch_seq,
                    TripBranchNumbers.tag,
	                TripRecords.driver_name,
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
	                departure_load_img_path,
                    ai_model_name
                FROM t_trip_records AS TripRecords
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
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
            ";
            return sql;
        }

        /// <summary>
        /// データベース用便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordForDataTable(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var selectedTrips = LoadRecordConnectController.SelectedTripsSQL(models);
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
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day,
                    ai_model_name,
                    unlinked_reason_name
                FROM t_trip_records AS TripRecords
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
                LEFT OUTER JOIN
                m_unlinked_reasons AS UnlinkedReasons
                ON
                TripRecords.unlinked_reason_id = UnlinkedReasons.unlinked_reason_id
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ORDER BY trip_name, work_day, trip_branch_seq
            ";
            return sql;
        }

        /// <summary>
        /// 画像出力用の便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecordForImage(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var selectedTrips = LoadRecordConnectController.SelectedTripsSQL(models);
            var sql = $@"
                SELECT
                    t_trip_records.trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                station_id,
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
                FROM t_trip_records
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ORDER BY arrived_at";
            return sql;
        }
    }
}

