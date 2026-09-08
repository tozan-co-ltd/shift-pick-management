namespace shift_pick_management.ConnectControllers
{
    public class ManagementPortalConnectController
    {
        /// <summary>
        /// 識別番号有、便名称無のデータを識別番号ごとに数えたデータを取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
        public static string CreateSQLToSelectCountNonTripNameRecordGroupByIdentifyNumber(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.identify_number
                       ,COUNT(identify_number) AS trip_count
                FROM t_trip_records AS TripRecords
                INNER JOIN
                    m_stations AS Depos
                ON 
                    TripRecords.station_id = Depos.station_id
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
                AND TripRecords.trip_id IS NULL
                AND TripRecords.identify_number IS NOT NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                GROUP BY identify_number
                ORDER BY trip_count DESC
            ";
            return sql;
        }

        /// <summary>
        /// 識別番号有、便マスターに該当する識別番号の便ありのデータの紐づけ切れ回数と紐付け有回数を取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
        public static string CreateSQLToSelectCountNonTripNameRecordAndInTripMaster(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TOP 10 *
                FROM(
                    SELECT 
                    Trips.trip_id,
                    Trips.trip_name,
	                TripRecords.identify_number,
                    COUNT(CASE WHEN TripRecords.trip_name IS NULL THEN 1 END) AS no_name_count,
                    COUNT(CASE WHEN TripRecords.trip_name IS NOT NULL THEN 1 END) AS trip_count,
                    Depos.name AS depo_name
                    FROM t_trip_records AS TripRecords
                    INNER JOIN
                    m_trucks AS Trucks
                    ON TripRecords.identify_number = Trucks.identify_number
                    INNER JOIN
                    m_trip_histories AS TripHistories
                    ON Trucks.truck_id = TripHistories.truck_id
                    INNER JOIN
                    m_trips AS Trips
                    ON TripHistories.trip_id = Trips.trip_id
                    INNER JOIN
                    m_depos As Depos
                    ON TripHistories.depo_id = Depos.depo_id
                    WHERE TripRecords.identify_number IS NOT NULL
                    AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                    AND applicable_start_datetime < '{startOfPeriod}'
                    AND applicable_end_datetime > '{endOfPeriod}'
                    {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                    GROUP BY TripRecords.identify_number, Trips.trip_name, Trips.trip_id, Depos.name, TripRecords.identify_number
                ) AS t1
                WHERE no_name_count > 0
                ORDER BY no_name_count DESC, trip_count DESC
            ";
            return sql;
        }

        /// <summary>
        /// 紐づけ切れ原因の一覧とそれぞれの回数を取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択したデポ</param>
        /// <returns></returns>
        public static string CreateSQLToSelectNonTripNameRemarkCount(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.unlinked_reason_id
                       ,unlinked_reason_name
                       ,COUNT(TripRecords.unlinked_reason_id) AS unlinked_reason_count
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
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
                AND TripRecords.trip_id IS NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                GROUP BY TripRecords.unlinked_reason_id, unlinked_reason_name
                ORDER BY unlinked_reason_count DESC
            ";
            return sql;
        }

    }
}
