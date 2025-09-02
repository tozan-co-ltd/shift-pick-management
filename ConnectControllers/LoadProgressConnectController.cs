using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadProgressConnectController
    {

        public static string CreateSQLToSelectM_Trips()
        {
            var sql = $@"
                SELECT 
                    TripHistories.trip_id,
                    TripHistories.trip_history_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_id,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    CONVERT(DATETIME, TripHistories.day_shift_start_time) AS day_shift_start_time,
	                TripHistories.depo_id,
	                Depos.name AS depo_name,
                    TripHistories.applicable_start_datetime,
                    TripHistories.applicable_end_datetime,
                    TripHistories.updated_at,
                    TripHistories.updated_by
                FROM 
                    m_trip_histories as TripHistories
                INNER JOIN
                    m_trips as Trips
                ON 
                    TripHistories.trip_id = Trips.trip_id
                INNER JOIN
                    m_trucks as Trucks
                ON
                    TripHistories.truck_id = Trucks.truck_id
                INNER JOIN 
	                m_depos as Depos
                ON
	                TripHistories.depo_id = Depos.depo_id
                ORDER BY
                    Trips.trip_name
            ";
            return sql;
        }

        public static string CreateSQLToSelectM_TripsFromDepo(int depoId)
        {
            var sql = $@"
                SELECT 
                    TripHistories.trip_id,
                    TripHistories.trip_history_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_id,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    CONVERT(DATETIME, TripHistories.day_shift_start_time) AS day_shift_start_time,
	                TripHistories.depo_id,
	                Depos.name AS depo_name,
                    TripHistories.applicable_start_datetime,
                    TripHistories.applicable_end_datetime,
                    TripHistories.updated_at,
                    TripHistories.updated_by
                FROM 
                    m_trip_histories as TripHistories
                INNER JOIN
                    m_trips as Trips
                ON 
                    TripHistories.trip_id = Trips.trip_id
                INNER JOIN
                    m_trucks as Trucks
                ON
                    TripHistories.truck_id = Trucks.truck_id
                INNER JOIN 
	                m_depos as Depos
                ON
	                TripHistories.depo_id = Depos.depo_id
                WHERE
                    TripHistories.depo_id = {depoId}
                ORDER BY
                    Trips.trip_name
            ";
            return sql;
        }

        public static string CreateSQLToSelectTripBranchNumbersFromDepo(int depoId, DateTime workDay)
        {
            var sql = $@"
                SELECT
	                BranchNumbers.trip_id,
	                trip_name,
                    trip_branch_number_id,
                    tag,
                    CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
                    CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
                    BranchNumbers.applicable_start_datetime,
                    BranchNumbers.applicable_end_datetime,
                    BranchNumbers.updated_at,
                    BranchNumbers.updated_by,
                    CONVERT(DATETIME, day_shift_start_time) AS day_shift_start_time
                FROM m_trip_branch_numbers AS BranchNumbers
                INNER JOIN m_trip_histories AS TripHistories
                ON BranchNumbers.trip_id = TripHistories.trip_id
                INNER JOIN m_trips AS Trips
                ON BranchNumbers.trip_id = Trips.trip_id
                WHERE BranchNumbers.applicable_end_datetime > '{workDay.ToString("yyyy/MM/dd HH:mm")}'
                AND TripHistories.applicable_end_datetime > '{workDay.ToString("yyyy/MM/dd HH:mm:ss")}'
                AND BranchNumbers.applicable_start_datetime < '{workDay.ToString("yyyy/MM/dd HH:mm")}'
                AND TripHistories.depo_id = {depoId}
                ORDER BY trip_name, arrival_scheduled_time
            ";
            return sql;
        }

        public static string CreateSQLToSelectLoadRecordsFromWorkDay(DateTime workDay)
        {
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
                    departure_load_img_path
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
                WHERE
                    work_day = '{workDay.ToString("yyyy/MM/dd")}'
                AND
                    identify_number IS NOT NULL
                ORDER BY trip_name
            ";
            return sql;
        }

        public static string CreateSQLToSelectLoadRecordsFromDepoAndWorkDay(int depoId, DateTime workDay)
        {
            var sql = $@"
                SELECT
                    trip_record_id,
                    trip_name,
                    trip_branch_seq,
                    TripBranchNumbers.tag,
                    TripRecords.driver_name,
                    TripRecords.station_id AS station_id,
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
                WHERE
                    Depos.depo_id = {depoId}
                AND
                    work_day = '{workDay.ToString("yyyy/MM/dd")}'
                AND
                    identify_number IS NOT NULL
                ORDER BY trip_name
            ";
            return sql;
        }

        public static string CreateSQLToSelectTripNames()
        {
            var sql = $@"
                SELECT
                    trip_name
                FROM
                    m_trips
            ";
            return sql;
        }

        /// <summary>
        /// ステーション毎のトラック有無取得SQL
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectIsExistTrucksFromStationID(int stationID)
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
                WHERE truck_sensor_records.station_id = {stationID}
            ";
            return sql;
        }

        public static string CreateSQLToSelectLatestTripRecordsFromStationID(int stationID)
        {
            var sql = $@"
                SELECT TripRecords.trip_record_id
	                ,TripRecords.station_id
	                ,TripRecords.created_at
                FROM t_trip_records AS TripRecords
                JOIN(
	                SELECT station_id, MAX(created_at) AS latest_create
	                FROM t_trip_records
	                GROUP BY station_id
                ) LatestTripRecords
                ON TripRecords.station_id = LatestTripRecords.station_id
                AND TripRecords.created_at = LatestTripRecords.latest_create
                WHERE TripRecords.station_id = {stationID}
            ";
            return sql;
        }
    }
}
