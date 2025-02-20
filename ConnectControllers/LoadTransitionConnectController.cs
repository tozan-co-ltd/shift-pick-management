using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadTransitionConnectController 
    {
        /// <summary>
        /// 便実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<LoadTransitionModel> ConnectTTripRecords(string sql)
        {
            // 戻り値
            List<LoadTransitionModel> strList = new();

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
                    strList = connection.Query<LoadTransitionModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }



        

        /// <summary>
        /// 検索条件から荷量クラスを取得するSQL
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <param name="tripBranchSeq">便枝番</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditions(string tripName, int tripBranchSeq, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var sql = $@"
                SELECT 
                    work_day,
	                arrival_load_class,
	                departure_load_class
                FROM t_trip_records
                WHERE trip_name = '{tripName}'
                AND trip_branch_seq = '{tripBranchSeq}'
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
            ";
            return sql;
        }

        /// <summary>
        /// 検索条件から荷量クラスを取得するSQL
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectLoadClassFromSearchConditionsForTable(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var selectedTrips= LoadRecordConnectController.SelectedTripsSQL(models);
            var sql = $@"
                SELECT 
                    trip_record_id,
	                trip_name,
	                trip_branch_seq,
	                TripRecords.driver_name,
	                station_id,
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
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
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
        public static string CreateSQLToSelectTripRecordForDataTable(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var selectedTrips = LoadRecordConnectController.SelectedTripsSQL(models);
            var sql = $@"
                SELECT
	                trip_name,
	                trip_branch_seq,
	                TripRecords.driver_name,
	                station_id,
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
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
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
        public static string CreatSQLToSelectTripRecordForImage(List<LoadRecordModel> models, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var selectedTrips = LoadRecordConnectController.SelectedTripsSQL(models);
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
                WHERE ({selectedTrips})
                AND work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ORDER BY arrived_at";
            return sql;
        }
    }
}

