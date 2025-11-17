namespace ai_truck_load_measurement.ConnectControllers
{
    public class ManagementPortalConnectController
    {
        /// <summary>
        /// 識別番号有、便名称無のデータを識別番号ごとに数えたデータを取得するSQL
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepos"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectNonTripNameRecordCount(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
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

        public static string CreateSQLToSelectNonTripNameRemarkCount(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.remark
                       ,COUNT(remark) AS remark_count
	                   ,Stations.depo_id
                       ,Depos.name AS depo_name
                FROM t_trip_records AS TripRecords
                INNER JOIN
                    m_stations AS Stations
                ON 
                    TripRecords.station_id = Stations.station_id
                INNER JOIN
	                m_depos AS Depos
                ON
	                Stations.depo_id = Depos.depo_id
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
                AND TripRecords.trip_id IS NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                GROUP BY remark, Stations.depo_id, Depos.name
            ";
            return sql;
        }

        public static string CreateSQLToSelectRemarks()
        {
            var sql = $@"
                SELECT TripRecords.remark
                FROM t_trip_records AS TripRecords

                WHERE TripRecords.work_day BETWEEN '2025/10/1' AND '2025/10/31'
                AND TripRecords.trip_id IS NULL
                GROUP BY remark
            ";
            return sql;
        }
    }
}
