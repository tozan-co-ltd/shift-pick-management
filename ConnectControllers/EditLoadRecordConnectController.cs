namespace ai_truck_load_measurement.ConnectControllers
{
    public class EditLoadRecordConnectController
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
                    remark,
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
        /// 指定した便実績IDの便実績取得SQL
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordFromID(int tripRecordID)
        {
            var sql = $@"
                SELECT 
                    TripRecords.trip_record_id,
	                trip_name,
                    TripRecords.trip_branch_number_id,
	                TripRecords.trip_branch_seq,
	                driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
	                CONVERT(DATETIME, TripRecords.arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, TripRecords.departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class AS revision_arrival_load_class,
	                departure_load_class AS revision_departure_load_class,
                    remark,
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
                WHERE trip_record_id = {tripRecordID}
            ";
            return sql;
        }

        public static string CreateSQLToSelectIdentifyNumbers()
        {
            var sql = $@"
                SELECT
                    identify_number
                FROM m_trucks
                ORDER BY identify_number
            ";
            return sql;
        }


        public static string CreateSQLToSelectTripBranchSeqsFromIdentifyNumber(string identifyNumber, DateTime arrivedAt)
        {
            var sql = $@"
                SELECT 
	                CONVERT(DATETIME, day_shift_start_time) AS day_shift_start_time
	                ,trip_branch_number_id
	                ,CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time
                FROM m_trip_histories AS TripHistories
                INNER JOIN
                m_trucks AS Trucks
                ON TripHistories.truck_id = Trucks.truck_id
                INNER JOIN
                m_trip_branch_numbers AS TripBranchNumbers
                ON TripHistories.trip_id = TripBranchNumbers.trip_id
                WHERE identify_number = '{identifyNumber}'
                AND TripHistories.applicable_end_datetime > '{arrivedAt}'
                AND TripHistories.applicable_start_datetime < '{arrivedAt}'
                AND TripBranchNumbers.applicable_end_datetime > '{arrivedAt}'
                AND TripBranchNumbers.applicable_start_datetime < '{arrivedAt}'
                ORDER BY arrival_scheduled_time
            ";
            return sql;
        }

        public static string CreateSQLToSelectTripNameFromIdentifyNumber(string identifyNumber, DateTime arrivedAt)
        {
            var sql = $@"
                SELECT 
	                trip_name
                FROM m_trip_histories AS TripHistories
                INNER JOIN
                m_trucks AS Trucks
                ON TripHistories.truck_id = Trucks.truck_id
                INNER JOIN
                m_trips AS Trips
                ON TripHistories.trip_id = Trips.trip_id
                WHERE identify_number = '{identifyNumber}'
                AND TripHistories.applicable_end_datetime > '{arrivedAt}'
                AND TripHistories.applicable_start_datetime < '{arrivedAt}'
            ";
            return sql;
        }
    }
}
