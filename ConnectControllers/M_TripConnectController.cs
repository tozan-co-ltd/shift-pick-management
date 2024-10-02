using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System.Data;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_TripConnectController 
    {
        /// <summary>
        /// 便情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_TripModel> ConnectMTrips(string sql, string databaseName)
        {
            // 戻り値
            List<M_TripModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
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
        /// 便マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrips()
        {
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    CONVERT(DATETIME, TripHistories.day_shift_start_time) AS day_shift_start_time,
                    TripHistories.applicable_start_datetime,
                    TripHistories.applicable_end_datetime,
                    TripHistories.updated_at,
                    TripHistories.updated_by
                FROM 
	                m_trip_histories as TripHistories
                INNER JOIN
                    m_trips as Trips
                ON 
                    TripHistories.trip_id = Trips.trip_id
                INNER JOIN
                    m_trucks as Trucks
                ON
                    TripHistories.truck_id = Trucks.truck_id
            ";
            return sql;
        }

        /// <summary>
        /// 重複便名称取得SQL作成
        /// </summary>
        /// <param name="truckNumber">車両コード</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMTripName(M_TripModel model)
        {
            var sql = $@"
                SELECT 
	                applicable_start_datetime,
	                applicable_end_datetime
                FROM m_trip_histories AS Histories
                INNER JOIN
	                m_trips AS Trips
                ON
	                Histories.trip_id = Trips.trip_id
                WHERE
	                Trips.trip_name = '{model.TripName}'
            ";

            return sql;
        }
    }
}
