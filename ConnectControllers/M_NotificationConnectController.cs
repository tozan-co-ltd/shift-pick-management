using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_NotificationConnectController 
    {
        /// <summary>
        /// 便情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_TripModel> ConnectMTrips(string sql)
        {
            // 戻り値
            List<M_TripModel> strList = new();

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
                    strList = connection.Query<M_TripModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便枝番情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_TripBranchNumberModel> ConnectMTripBranchNumbers(string sql)
        {
            // 戻り値
            List<M_TripBranchNumberModel> strList = new();

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
                    strList = connection.Query<M_TripBranchNumberModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrips()
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                SELECT 
                    TripHistories.trip_id,
                    Trips.trip_name,
	                TripHistories.depo_id,
	                Depos.name AS depo_name,
                    TripHistories.applicable_start_datetime,
                    TripHistories.applicable_end_datetime
                FROM 
                    m_trip_histories as TripHistories
                INNER JOIN
                    m_trips as Trips
                ON 
                    TripHistories.trip_id = Trips.trip_id
                INNER JOIN 
	                m_depos as Depos
                ON
	                TripHistories.depo_id = Depos.depo_id
                WHERE
                        TripHistories.applicable_end_datetime > '{today}'
                    ORDER BY 
                        TripHistories.trip_id, TripHistories.applicable_start_datetime
            ";
            return sql;
        }

        /// <summary>
        /// 便枝番情報取得SQL作成
        /// </summary>
        /// <param name="tripId">便ID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMTripBranchNumbers(int tripId)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                SELECT
	                trip_branch_number_id,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                TripBranchNumbers.applicable_start_datetime,
	                TripBranchNumbers.applicable_end_datetime,
	                CONVERT(DATETIME, day_shift_start_time) AS day_shift_start_time
                FROM m_trip_branch_numbers AS TripBranchNumbers
                INNER JOIN m_trip_histories AS TripHistories
                ON TripBranchNumbers.trip_id = TripHistories.trip_id
                WHERE TripBranchNumbers.trip_id = '{tripId}'
                AND TripBranchNumbers.applicable_end_datetime > '{today}'
            ";
            return sql;
        }
        /// <summary>
        /// 便枝番情報取得SQL作成
        /// </summary>
        /// <param name="tripId">便ID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMTripBranchNumberFromTripBranchNumberID(int tripId, int tripBranchNumberID)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                SELECT
	                trip_branch_number_id,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                applicable_start_datetime,
	                applicable_end_datetime
                FROM m_trip_branch_numbers
                WHERE trip_id = '{tripId}'
                AND trip_branch_number_id = '{tripBranchNumberID}'
                AND applicable_end_datetime > '{today}'
            ";
            return sql;
        }
    }
}
