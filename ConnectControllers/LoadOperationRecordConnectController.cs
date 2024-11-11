namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadOperationRecordConnectController 
    {
        public static string CreateSQLToSelectTripNameFromWorkDays(DateTime workDay)
        {
            var sql = $@"
                SELECT DISTINCT
	                trip_name AS Value,
	                trip_name AS Text
                FROM t_trip_records
                WHERE work_day = '{workDay.ToString("yyyy/MM/dd")}'
                AND trip_name IS NOT NULL
";
            return sql;
        }

        private static string SelectedDaysSQL(List<DateTime> days)
        {
            var selectedDays = "";
            for(int i=0; i<days.Count; i++)
            {
                if(i != 0)
                {
                    selectedDays += " OR ";
                }
                selectedDays += $"work_day = '{days[i].ToString("yyyy/MM/dd")}'";
            }
            return selectedDays;
        }
    }
}
