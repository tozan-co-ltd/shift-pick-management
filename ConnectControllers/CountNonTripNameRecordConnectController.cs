namespace shift_pick_management.ConnectControllers
{
    public class CountNonTripNameRecordConnectController
    {
        public static string CreateSQLToSelectCountNonTripNameRecord(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos, bool isNoNameCountNot0)
        {
            var sql = $@"
                SELECT *
                FROM(
                    SELECT 
                    Trips.trip_id,
                    Trips.trip_name,
                    COUNT(CASE WHEN TripRecords.trip_name IS NULL THEN 1 END) AS no_name_count,
                    COUNT(*) AS trip_count,
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
                    GROUP BY TripRecords.identify_number, Trips.trip_name, Trips.trip_id, Depos.name
                ) AS t1
                {IsNoNameCountNot0String(isNoNameCountNot0)}
                ORDER BY trip_id
            ";
            return sql;
        }
        private static string IsNoNameCountNot0String(bool isNoNameCountNot0)
        {
            var sql = "";
            if (isNoNameCountNot0)
            {
                sql = "WHERE no_name_count > 0 ";
            }

            return sql;
        }
    }

    
}
