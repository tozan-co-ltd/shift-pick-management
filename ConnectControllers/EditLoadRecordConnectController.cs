using ai_truck_load_measurement.Models;

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
                    TripRecords.unlinked_reason_id,
                    unlinked_reason_name,
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
                LEFT OUTER JOIN
                m_unlinked_reasons AS UnlinkedReasons
                ON
                TripRecords.unlinked_reason_id = UnlinkedReasons.unlinked_reason_id
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
                    TripRecords.trip_id,
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
                    unlinked_reason_id,
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

        /// <summary>
        /// 識別番号一覧取得SQL
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 識別番号から便枝番マスタ情報を取得するSQL
        /// </summary>
        /// <param name="identifyNumber"></param>
        /// <param name="arrivedAt"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 識別番号から便名称を取得するSQL
        /// </summary>
        /// <param name="identifyNumber"></param>
        /// <param name="arrivedAt"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripNameFromIdentifyNumber(string identifyNumber, DateTime arrivedAt)
        {
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    trip_name,
	                driver_name,
	                truck_number
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

        /// <summary>
        /// 便枝番IDから到着・出発予定時間を取得するSQL
        /// </summary>
        /// <param name="tripBranchNumberID"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectScheduledTimeFromTripBranchNumberID(int tripBranchNumberID)
        {
            var sql = $@"
            SELECT
                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time
                ,CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time
            FROM m_trip_branch_numbers
            WHERE trip_branch_number_id = {tripBranchNumberID}
            ";
            return sql;
        }

        /// <summary>
        /// 便実績更新SQL
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string CreateSQLToUpdateTripRecord(EditLoadRecordModel model)
        {
            var sql = $@"
                UPDATE t_trip_records
                SET
            ";
            if (model.TripID != 0)
            {
                sql += $@"
                    trip_id = {model.TripID},
                    trip_name = '{model.TripName}',
                    driver_name = '{model.DriverName}',
                    truck_number = '{model.TruckNumber}',
                    identify_number = '{model.IdentifyNumber}',
                ";
            }
            if(model.TripBranchNumberID != 0)
            {
                sql += $@"
                    trip_branch_number_id = {model.TripBranchNumberID},
                    trip_branch_seq = '{model.TripBranchSeq}',
                    arrival_scheduled_time = '{model.RegistArrivalScheduledTime}',
                    departure_scheduled_time = '{model.RegistDepartureScheduledTime}',
                ";
            }
            if (!string.IsNullOrEmpty(model.DepartureLoadImgPath))
            {
                sql += $@"
                    departure_load_img_path = '{model.DepartureLoadImgPath}',
                ";
            }
            if (model.DepartedAt.ToString("yyyy/MM/dd HH:mm:ss") != "0001/01/01 00:00:00")
            {
                sql += $@"
                    departed_at = '{model.DepartedAt.ToString("yyyy/MM/dd HH:mm:ss")}',
                ";
            }
            sql += $@"
                    arrived_at = '{model.ArrivedAt.ToString("yyyy/MM/dd HH:mm:ss")}',
                    arrival_load_img_path = '{model.ArrivalLoadImgPath}',
                    unlinked_reason_id = {model.UnlinkedReasonID},
                    is_deleted = '{model.IsDeleted}'
                WHERE trip_record_id = {model.TripRecordID}
            ";
                return sql;
        }

        /// <summary>
        /// 紐づけ切れ原因リスト取得SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectUnlinkedReasons()
        {
            var sql = $@"
                SELECT *
                FROM m_unlinked_reasons
            ";
            return sql;
        }

        public static string CreateSQLToInsertCorrectionItems(CorrectionItemModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO t_correction_items(
                    trip_record_id, 
                    is_identify_number_changed,
                    before_identify_number,
                    is_trip_id_changed,
                    before_trip_id,
                    is_trip_branch_number_id_changed,
                    before_trip_branch_number_id,
                    is_arrived_at_changed,
                    before_arrived_at,
                    is_departed_at_changed,
                    before_departed_at,
                    is_arrival_load_img_path_changed,
                    before_arrival_load_img_path,
                    is_departure_load_img_path_changed,
                    before_departure_load_img_path,
                    created_at,
                    created_by
                )
                VALUES (
                    '{model.TripRecordID}',
                    '{model.IsIdentifyNumberChanged}',
                    '{model.BeforeIdentifyNumber}',
                    '{model.IsTripIDChanged}',
                    '{model.BeforeTripID}',
                    '{model.IsTripBranchNumberIDChanged}',
                    '{model.BeforeTripBranchNumberID}',
                    '{model.IsArrivedAtChanged}',
                    '{model.BeforeArrivedAt}',
                    '{model.IsDepartedAtChanged}',
                    '{model.BeforeDepartedAt}',
                    '{model.IsArrivalLoadImgPathChanged}',
                    '{model.BeforeArrivalLoadImgPath}',
                    '{model.IsDepartureLoadImgPathChanged}',
                    '{model.BeforeDepartureLoadImgPath}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }
    }
}
