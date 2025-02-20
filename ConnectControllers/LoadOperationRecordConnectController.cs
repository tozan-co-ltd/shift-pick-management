using ai_truck_load_measurement.Commons;
using System.Data.SqlClient;
using ai_truck_load_measurement.Models;
using Dapper;


namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadOperationRecordConnectController 
    {
        /// <summary>
        /// 便実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<LoadOperationRecordModel> ConnectTTripRecords(string sql)
        {
            // 戻り値
            List<LoadOperationRecordModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    strList = connection.Query<LoadOperationRecordModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 稼働日から便名称を取得するSQL
        /// </summary>
        /// <param name="workDays">稼働日のリスト</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripNameFromWorkDays(List<DateTime> workDays, List<string> checkedDepos)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT DISTINCT
	                trip_name AS Value,
	                trip_name AS Text
                FROM t_trip_records AS TripRecords
                INNER JOIN
	                m_trip_histories AS TripHistories
                ON 
	                TripRecords.trip_id = TripHistories.trip_id
                WHERE ({selectedDays})
                AND trip_name IS NOT NULL
            ";
            if (checkedDepos[0] != "0")
            {
                sql += "AND (";
                for (int i = 0; i < checkedDepos.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += $" OR ";
                    }
                    sql += $"depo_id = {checkedDepos[i]}";
                }
                sql += ")";
            }
            return sql;
        }

        /// <summary>
        /// 稼働日のリストをSQLのWHERE文に変換する
        /// </summary>
        /// <param name="days">稼働日のリスト</param>
        /// <returns></returns>
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

        /// <summary>
        /// 条件から荷量クラスと昼勤開始時間を取得するSQL
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <param name="workDay">稼働日</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditions(string tripName, DateTime workDay)
        {
            var sql = $@"
                SELECT
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
                    work_day,
	                arrival_load_class,
	                departure_load_class,
	                arrived_at,
	                departed_at,
                    CONVERT(DATETIME, histories.day_shift_start_time) AS day_shift_start_time
                FROM t_trip_records AS trip_records
                INNER JOIN m_trips AS trips
                ON trip_records.trip_name = trips.trip_name
                INNER JOIN m_trip_histories AS histories
                ON trips.trip_id = histories.trip_id
                WHERE work_day = '{workDay.ToString("yyyy/MM/dd")}'
                AND trip_records.trip_name = '{tripName}'
            ";
            return sql;
        }

        /// <summary>
        /// 検索条件から便実績を取得するSQL
        /// </summary>
        /// <param name="workDays">稼働日</param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditionsForTable(List<DateTime> workDays, string tripName)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT 
                    trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                TripRecords.driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
                    Depos.name AS depo_name,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                work_day,
	                arrived_at,
	                departed_at,
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
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
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                WHERE ({selectedDays})
                AND trip_name = '{tripName}'
            ";
            return sql;
        }

        /// <summary>
        /// データテーブル用便実績情報取得SQL
        /// </summary>
        /// <param name="workDays">稼働日</param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordForDataTable(List<DateTime> workDays, string tripName)
        {
            var selectedDays = SelectedDaysSQL(workDays);
            var sql = $@"
                SELECT
	                trip_name,
	                trip_branch_seq,
	                TripRecords.driver_name,
	                Stations.name AS station_name,
	                truck_number,
	                identify_number,
                    Depos.name AS depo_name,
	                FORMAT(CONVERT(DATETIME, arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time,
	                FORMAT(CONVERT(DATETIME, departure_scheduled_time), 'HH:mm') AS departure_scheduled_time,
	                FORMAT(work_day, 'yyyy/MM/dd') AS work_day,
	                FORMAT(arrived_at, 'yyyy/MM/dd HH:mm'),
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm'),
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
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
                m_stations AS Stations
                ON
                TripRecords.station_id = Stations.station_id
                WHERE ({selectedDays})
                AND trip_name = '{tripName}'
                ORDER BY trip_name, work_day, trip_branch_seq
            ";
            return sql;
        }

        /// <summary>
        /// 画像出力用の便実績情報取得SQL
        /// </summary>
        /// <param name="workDays">稼働日</param>
        /// <param name="tripName">便名称</param>
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
