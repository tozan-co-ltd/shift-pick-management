using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class ArrivalTimeDefferenceConnectController
    {
        public static string CreateSQLToSelectArrivalRecordAndSchedule(List<SelectedTripModel> trips, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var sql = $@"
                SELECT
                    trip_id
                    ,trip_name
                    ,trip_branch_seq
                    ,CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time
                    ,work_day
                    ,arrived_at
                FROM
                    t_trip_records
                WHERE
                    work_day BETWEEN '{startOfPeriod.ToString("yyyy/MM/dd")}' AND '{endOfPeriod.ToString("yyyy/MM/dd")}'
                    {CreateSQLToTripConditions(trips)}
                ";
            return sql;
        }

        private static string CreateSQLToTripConditions(List<SelectedTripModel> trips)
        {
            var sql = "AND (";
            if(trips.Count == 0)
            {
                sql += "1=0)";
                return sql;
            }

            for (int i = 0; i < trips.Count; i++)
            {
                if (i != 0)
                    sql += "OR";

                sql += $@"(trip_name = '{trips[i].TripName}' AND trip_branch_seq = {trips[i].TripBranchSeq})
                ";
            }

            sql += ")";
            return sql;
        }
    }
}
