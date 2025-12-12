using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class CountAlertRecordConnectController
    {
        public static string CreateSQLToSelectNotifications(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> selectedTrips)
        {
            var sql = $@"
                SELECT *
                FROM m_notifications AS Notifications
                INNER JOIN m_trips AS Trips
                ON Trips.trip_id = Notifications.trip_id
                WHERE notification_start_datetime < '{startOfPeriod.ToString("yyyy/MM/dd")}'
                AND notification_end_datetime > '{endOfPeriod.ToString("yyyy/MM/dd")}'
                {CreateSQLToSelectedTrips(selectedTrips)}
            ";
            return sql;
        }

        private static string CreateSQLToSelectedTrips(List<SelectedTripModel> selectedTrips)
        {
            var sql = "";
            if (selectedTrips.Count == 0)
                return sql;

            for(int i=0; i<selectedTrips.Count; i++)
            {
                if (i == 0)
                    sql += "AND (";
                else
                    sql += "OR ";

                sql += $@"(trip_name = '{selectedTrips[i].TripName}' AND trip_branch_seq = {selectedTrips[i].TripBranchSeq})
                ";
            }
            sql += ")";
            return sql;
        }

        public static string CreateSQLToSelectTripNameFromPeriodAndNotifications(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT DISTINCT
                    trip_name,
                    TripRecords.trip_branch_seq,
                    TripHistories.depo_id,
                    name As depo_name
                FROM t_trip_records AS TripRecords
                INNER JOIN
                    m_trip_histories AS TripHistories
                ON 
                    TripRecords.trip_id = TripHistories.trip_id
                INNER JOIN
                    m_depos AS Depos
                ON
                    TripHistories.depo_id = Depos.depo_id
                INNER JOIN
	                m_notifications AS Notifications
                ON
	                TripHistories.trip_id = Notifications.trip_id
                AND TripRecords.trip_branch_seq = Notifications.trip_branch_seq
                WHERE work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
                AND trip_name IS NOT NULL
                AND notification_start_datetime < '{startOfPeriod.ToString("yyyy/MM/dd")}'
                AND notification_end_datetime > '{endOfPeriod.ToString("yyyy/MM/dd")}'
                {LoadRecordConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            return sql;
        }

        public static string CreateSQLToSelectTripRecordCount(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> selectedTrips)
        {
            var sql = $@"
                SELECT 
                    trip_id
                    ,trip_name
	                ,trip_branch_seq
	                ,COUNT(trip_record_id) AS trip_record_count
                FROM t_trip_records
                WHERE work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                {CreateSQLToSelectedTrips(selectedTrips)}
                GROUP BY trip_id, trip_name, trip_branch_seq
            ";
            return sql;
        }
    }
}
