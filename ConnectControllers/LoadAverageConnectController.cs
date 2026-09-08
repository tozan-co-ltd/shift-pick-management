using shift_pick_management.Models;

namespace shift_pick_management.ConnectControllers
{
    public class LoadAverageConnectController
    {
        public static string CreateSQLToSelectLoadClasses(List<SelectedTripModel> trips, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var sql = $@"
                SELECT
                    trip_id
                    ,trip_name
	                ,arrival_load_class
	                ,departure_load_class
                    ,trip_branch_seq
                    ,work_day
                FROM
                    t_trip_records
                WHERE
                    work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
                {LoadRecordConnectController.CreateSQLToTripConditions(trips)}
            ";
            return sql;
        }

        public static string CreateSQLToSelectNotificationsFromTrips(List<SelectedTripModel> trips, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var sql = $@"
                SELECT
                    Notifications.trip_id
                    ,trip_name
                    ,trip_branch_seq
                    ,arrival_lower_load_class
                    ,departure_lower_load_class
                FROM
                    m_notifications AS Notifications
                INNER JOIN 
	                m_trips AS Trips
                ON Notifications.trip_id = Trips.trip_id
                WHERE 
	                notification_start_datetime <= '{startOfPeriod}'
                AND notification_end_datetime >= '{endOfPeriod}'
                {LoadRecordConnectController.CreateSQLToTripConditions(trips)}
            ";
            return sql;
        }
    }
}
