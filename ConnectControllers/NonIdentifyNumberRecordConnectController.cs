namespace ai_truck_load_measurement.ConnectControllers
{
    public class NonIdentifyNumberRecordConnectController
    {
        public static string CreateSQLToSelectNonIdentifyNumberRecords(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT
                    trip_record_id,
                    arrived_at,
                    departed_at,
                    work_day,
	                Stations.name AS station_name,
                    TripRecords.unlinked_reason_id,
                    unlinked_reason_name,
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
                m_unlinked_reasons AS UnlinkedReasons
                ON
                TripRecords.unlinked_reason_id = UnlinkedReasons.unlinked_reason_id
                WHERE identify_number IS NULL
                AND is_deleted = 0
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            return sql;
        }

        public static string CreateSQLToSelectNonIdentifyNumberRecordsForDataTable(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT
                    trip_record_id, 
                    FORMAT(arrived_at, 'yyyy/MM/dd HH:mm') AS arrived_at,
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm') AS departed_at,
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day,
	                Stations.name AS station_name,
                    unlinked_reason_name,
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
                m_unlinked_reasons AS UnlinkedReasons
                ON
                TripRecords.unlinked_reason_id = UnlinkedReasons.unlinked_reason_id
                WHERE identify_number IS NULL
                AND is_deleted = 0
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            return sql;
        }

        /// <summary>
        /// 画像出力用の便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecordForImage(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT
                    TripRecords.trip_record_id,
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
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                AND identify_number IS NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                ORDER BY arrived_at
            ";
            return sql;
        }
    }
}
