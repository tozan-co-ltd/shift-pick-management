using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadOperationRecordConnectController 
    {
        public static string CreateSQLToSelectTripNameFromWorkDays(List<DateTime> workDays)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT DISTINCT
	                trip_name AS Value,
	                trip_name AS Text
                FROM t_trip_records
                WHERE ({selectedDays})
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

        public static string CreateSQLToSelectLoadClassFromSearchConditions(string tripName, DateTime workDay)
        {
            var sql = $@"
                SELECT
	                arrival_load_class,
	                departure_load_class,
	                arrived_at,
	                departed_at
                FROM t_trip_records
                WHERE work_day = '{workDay.ToString("yyyy/MM/dd")}'
                AND trip_name = '{tripName}'
            ";
            return sql;
        }

        /// <summary>
        /// 検索条件から荷量クラスを取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditionsForTable(List<DateTime> workDays, string tripName)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT 
                    trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                station_id,
	                truck_number,
	                identify_number,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                FROM t_trip_records
                WHERE ({selectedDays})
                AND trip_name = '{tripName}'
            ";
            return sql;
        }

        /// <summary>
        /// データベース用便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordForDataTable(List<DateTime> workDays, string tripName)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                station_id,
	                truck_number,
	                identify_number,
	                FORMAT(CONVERT(DATETIME, arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time,
	                FORMAT(CONVERT(DATETIME, departure_scheduled_time), 'HH:mm') AS departure_scheduled_time,
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day,
	                FORMAT(arrived_at, 'yyyy/MM/dd HH:mm'),
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm'),
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                FROM t_trip_records
                WHERE ({selectedDays})
                AND trip_name = '{tripName}'
                ORDER BY trip_name, work_day, trip_branch_seq
            ";
            return sql;
        }

        /// <summary>
        /// 画像出力用の便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecordForImage(List<DateTime> workDays, string tripName)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT
                    t_trip_records.trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                driver_name,
	                station_id,
	                truck_number,
	                identify_number,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                FROM t_trip_records
                WHERE ({selectedDays})
                AND trip_name = '{tripName}'
                ORDER BY arrived_at";
            return sql;
        }

    }
}
