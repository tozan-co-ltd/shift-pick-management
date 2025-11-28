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
                WHERE notification_start_datetime < '{startOfPeriod}'
                AND notification_end_datetime > '{endOfPeriod}'
                {CreateSQLToSelectedTrips(selectedTrips)}
            ";
            return sql;
        }

        private static string CreateSQLToSelectedTrips(List<SelectedTripModel> selectedTrips)
        {
            var sql = "";
            for(int i=0; i<selectedTrips.Count; i++)
            {
                if (i == 0)
                    sql += "AND ";
                else
                    sql += "OR ";

                sql += $@"(trip_name = '{selectedTrips[i].TripName}' AND trip_branche_seq = {selectedTrips[i].TripBranchSeq})
                ";
            }
            return sql;
        }
    }
}
