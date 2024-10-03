using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
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
        /// 便情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMTrip(M_TripModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString("AI-truck-load-measurement_test");
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // 便名称の重複チェックと便ID取得
                model.TripID = CheckDuplicateNameAndGetID(connection, model.TripName); 

                // 便履歴テーブルに登録
                var insertedCount = InsertMTripHistoryTable(connection, model, loginUser.UserName);

                return insertedCount;
            }
        }

        /// <summary>
        /// 便情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int UpdateMTrip(M_TripModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString("AI-truck-load-measurement_test");
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;

                    // 便名称の重複チェックと便ID取得
                    model.TripID = CheckDuplicateNameAndGetID(connection, model.TripName);

                    // 便履歴テーブル更新
                    string sql = CreateSQLToUpdateMTripHistory(model, sysDate, loginUser.UserName);
                    var count = connection.Execute(sql);
                    return count;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 同じ便名称のデータが便マスターに登録されているか
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        private static bool IsSameTripNameExist(SqlConnection connection, string tripName)
        {
            var isTripExist = false;

            try
            {
                string sql = CreateSQLToExistMTripName(tripName);
                var reader = connection.ExecuteScalar(sql);
                if (reader != null)
                {
                    isTripExist = true;
                }
                return isTripExist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便テーブルに新規登録
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName"></param>
        private static void InsertMTripTable(SqlConnection connection, string tripName)
        {
            try
            {
                string sql = CreateSQLToInsertMTrip(tripName);
                connection.Execute(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便名称から便ID取得
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        private static int SelectMTripID(SqlConnection connection, string tripName)
        {
            try
            {
                string sql = CreateSQLToSelectMTripID(tripName);
                var tripID = Convert.ToInt32(connection.ExecuteScalar(sql));
                return tripID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便履歴テーブルに登録
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="model">便履歴モデル</param>
        /// <param name="userName">登録者名</param>
        /// <returns></returns>
        private static int InsertMTripHistoryTable(SqlConnection connection, M_TripModel model, string userName)
        {
            try
            {
                DateTime sysDate = DateTime.Now;
                string sql = CreateSQLToInsertMTripHistory(model, sysDate, userName);
                var insertedCount = connection.Execute(sql);
                return insertedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便名称の重複チェックとID取得
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName"></param>
        /// <returns></returns>
        private static int CheckDuplicateNameAndGetID(SqlConnection connection, string tripName)
        {
            // 同じ便名称のデータが便マスターに登録されているか
            var isTripExist = IsSameTripNameExist(connection, tripName);

            // 便名称が便マスターに登録されていない場合
            // 便テーブルに新規登録
            if (!isTripExist)
            {
                InsertMTripTable(connection, tripName);
            }

            // 便名称から便ID取得
            var tripID = SelectMTripID(connection, tripName);
            return tripID;
        }

        /// <summary>
        /// 便マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrips(bool beforePeriod)
        {
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    TripHistories.trip_history_id,
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
            if (!beforePeriod)
            {
                DateTime today = DateTime.Now;
                string formatToday = today.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                    WHERE
                        TripHistories.applicable_end_datetime > '{today}'
                ";
            }
            return sql;
        }

        /// <summary>
        /// 重複便名称取得SQL作成
        /// </summary>
        /// <param name="truckNumber">便名称</param>
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
                AND
                    Histories.trip_history_id <> '{model.TripHistoryID}'
            ";

            return sql;
        }

        /// <summary>
        /// 同名の便の有無情報取得SQL
        /// </summary>
        /// <param name="tripName"></param>
        /// <returns></returns>
        private static string CreateSQLToExistMTripName(string tripName)
        {
            var sql = $@"
                SELECT TOP(1) *
                FROM m_trips
                WHERE trip_name = '{tripName}'
                    
            ";
            return sql;
        }

        /// <summary>
        /// 便名称から便IDを取得するSQL作成
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        private static string CreateSQLToSelectMTripID(string tripName)
        {
            var sql = $@"
                SELECT 
	                trip_id
                FROM m_trips
                WHERE trip_name = '{tripName}'
                    
            ";
            return sql;
        }

        /// <summary>
        /// 便テーブル登録SQL作成
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        private static string CreateSQLToInsertMTrip(string tripName)
        {
            var sql = $@"
                INSERT INTO m_trips(
                    trip_name
                )
                VALUES (
                    '{tripName}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 便履歴テーブル登録SQL作成
        /// </summary>
        /// <param name="model">便履歴</param>
        /// <param name="createdAt">作成時間</param>
        /// <param name="createdBy">作成者</param>
        /// <returns></returns>
        private static string CreateSQLToInsertMTripHistory(M_TripModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO m_trip_histories(
                    trip_id,
                    truck_id,
                    driver_name,
                    day_shift_start_time,
                    applicable_start_datetime,
                    applicable_end_datetime,
                    created_at,
                    created_by,
                    updated_at,
                    updated_by
                )
                VALUES (
                    '{model.TripID}',
                    '{model.SelectedTruckID}',
                    '{model.DriverName}',
                    '{model.DayShiftStartTime}',
                    '{model.ApplicableStartDateTime}',
                    '{model.ApplicableEndDateTime}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 便履歴テーブル更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMTripHistory(M_TripModel model, DateTime updatedAt, string updatedBy)
        {
            string formatupdatedAt = updatedAt.ToString("yyyy/MM/dd HH:mm:ss");
            var sql = $@"
                UPDATE m_trip_histories
                SET 
                    trip_id = '{model.TripID}',
	                driver_name = '{model.DriverName}',
	                truck_id = {model.SelectedTruckID},
	                day_shift_start_time = '{model.DayShiftStartTime}',
	                applicable_start_datetime = '{model.ApplicableStartDateTime}',
	                applicable_end_datetime = '{model.ApplicableEndDateTime}',
                    updated_at = '{formatupdatedAt}',
                    updated_by = '{updatedBy}'
                WHERE trip_history_id = {model.TripHistoryID}
            ";
            return sql;
        }

        /// <summary>
        /// 便テーブル更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMTrip(M_TripModel model)
        {
            var sql = $@"
                UPDATE m_trip
                SET 
	                trip_name = '{model.TripName}'
                WHERE trip_id = {model.TripID}
            ";
            return sql;
        }
    }
}
