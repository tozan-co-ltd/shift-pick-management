using ai_truck_load_measurement.Controllers;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class NonTripNameRecordConnectController : BaseController
    {
        public static string CreateSQLToSelectNonTripNameRecords(DateTime startOfPeriod , DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.trip_record_id
                      ,TripRecords.identify_number
                      ,TripRecords.work_day
                      ,TripRecords.arrived_at
                      ,TripRecords.is_deleted
                      ,TripRecords.unlinked_reason_id
                      ,unlinked_reason_name
                FROM t_trip_records AS TripRecords
               INNER JOIN
	                m_stations AS Depos
                ON 
	                TripRecords.station_id = Depos.station_id
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                LEFT OUTER JOIN
                m_unlinked_reasons AS UnlinkedReasons
                ON
                TripRecords.unlinked_reason_id = UnlinkedReasons.unlinked_reason_id
                AND TripRecords.trip_id IS NULL
                AND TripRecords.identify_number IS NOT NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            return sql;
        }

        public static string CreateSQLToSelectNonTripNameRecordsForDataTable(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.identify_number
                      ,TripRecords.work_day
                      ,TripRecords.arrived_at
                      ,TripRecords.remark
                FROM t_trip_records AS TripRecords
               INNER JOIN
	                m_stations AS Depos
                ON 
	                TripRecords.station_id = Depos.station_id
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                AND TripRecords.trip_id IS NULL
                AND TripRecords.identify_number IS NOT NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            return sql;
        }
        public static string CreateSQLToSelectNonTripNameIdentifyNumbers(DateTime startOfPeriod,  DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT TripRecords.identify_number
                FROM t_trip_records AS TripRecords
                INNER JOIN
	                m_stations AS Depos
                ON 
	                TripRecords.station_id = Depos.station_id
                WHERE TripRecords.work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                AND TripRecords.trip_id IS NULL
                AND TripRecords.identify_number IS NOT NULL
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
                GROUP BY identify_number
            ";
            return sql;
        }
    }
}
