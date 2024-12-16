using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_TripConnectController 
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
        /// 便情報と便履歴情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMTripAndMTripHistory(M_TripModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                using(var command = connection.CreateCommand())
                {
                    // トランザクションの開始
                    command.Transaction = connection.BeginTransaction();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    int insertedCount;

                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    try
                    {
                        // 便名称の便ID取得と重複チェックおよび新規登録
                        model.TripID = GetMTripIDAndDuplicateChecksAndInsertsForTripName(connection, command, model.TripName);

                        // 便履歴テーブルに登録
                        insertedCount = InsertMTripHistoryTable(connection, command, model, loginUser.UserName);
                    }
                    catch (Exception) { 
                        command.Transaction.Rollback();
                        throw;
                    }
                    command.Transaction.Commit();
                    return insertedCount;
                }
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
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                using(var command = connection.CreateCommand())
                {
                    // トランザクションの開始
                    command.Transaction = connection.BeginTransaction();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    int count;

                    // DB接続
                    try
                    {
                        DateTime sysDate = DateTime.Now;

                        // 便名称の便ID取得と重複チェックおよび新規登録
                        model.TripID = GetMTripIDAndDuplicateChecksAndInsertsForTripName(connection, command, model.TripName);

                        // 便履歴テーブル更新
                        string sql = CreateSQLToUpdateMTripHistory(model, sysDate, loginUser.UserName);
                        count = connection.Execute(sql, new {}, command.Transaction);
                    }
                    catch (Exception)
                    {
                        command.Transaction.Rollback();
                        throw;
                    }
                    command.Transaction.Commit();
                    return count;
                }
                
            }
        }

        /// <summary>
        /// 同じ便名称のデータが便マスターに登録されているか
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        private static bool IsSameTripNameExist(SqlConnection connection, SqlCommand command, string tripName)
        {
            // 戻り値
            var isTripExist = false;

            try
            {
                string sql = CreateSQLToExistMTripName(tripName);
                // 同じ便名称のデータが存在する場合、値が代入される
                var reader = connection.ExecuteScalar(sql,new { }, command.Transaction);
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
        private static void InsertMTripTable(SqlConnection connection, SqlCommand command, string tripName)
        {
            try
            {
                string sql = CreateSQLToInsertMTrip(tripName);
                connection.Execute(sql, new {}, command.Transaction);
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
        private static int SelectMTripID(SqlConnection connection, SqlCommand command, string tripName)
        {
            try
            {
                string sql = CreateSQLToSelectMTripID(tripName);
                var tripID = Convert.ToInt32(connection.ExecuteScalar(sql, new { }, command.Transaction));
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
        private static int InsertMTripHistoryTable(SqlConnection connection, SqlCommand command, M_TripModel model, string userName)
        {
            try
            {
                DateTime sysDate = DateTime.Now;
                string sql = CreateSQLToInsertMTripHistory(model, sysDate, userName);
                var insertedCount = connection.Execute(sql, new {}, command.Transaction);
                return insertedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便名称のID取得と重複チェック、及び新規登録
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tripName"></param>
        /// <returns></returns>
        private static int GetMTripIDAndDuplicateChecksAndInsertsForTripName(SqlConnection connection, SqlCommand command, string tripName)
        {
            // 同じ便名称のデータが便マスターに登録されているか
            var isTripExist = IsSameTripNameExist(connection, command, tripName);

            // 便名称が便マスターに登録されていない場合
            // 便テーブルに新規登録
            if (!isTripExist)
            {
                InsertMTripTable(connection, command, tripName);
            }

            // 便名称から便ID取得
            var tripID = SelectMTripID(connection, command, tripName);
            return tripID;
        }


        /// <summary>
        /// 車両IDから識別番号を取得
        /// </summary>
        /// <param name="truckID">車両ID</param>
        /// <returns></returns>
        public static string SelectIdentifyNumberByTruckId(int truckID)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                // DB接続
                try
                {
                    string sql = CreateSQLToSelectIdentifyNumberByTruckId(truckID);
                    var identifyNumber = Convert.ToString(connection.ExecuteScalar(sql));

                    return identifyNumber;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 便情報をデータテーブルとして取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static DataTable ConnectMTripsToDataTable(string sql)
        {
            // 戻り値
            DataTable dataTable = new DataTable();

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
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 便マスター情報取得SQL作成
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用終了日時を過ぎた便を表示するか</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrips(bool isBeforeApplicablePeriod)
        {
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    TripHistories.trip_history_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_id,
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
            // 適用終了日時を過ぎた便を表示しない場合
            if (!isBeforeApplicablePeriod)
            {
                DateTime today = DateTime.Now;
                string formatToday = today.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                    WHERE
                        TripHistories.applicable_end_datetime > '{today}'
                ";
            }

            sql += $@"
                    ORDER BY 
                        TripHistories.trip_id, TripHistories.applicable_start_datetime
                ";
            return sql;
        }


        /// <summary>
        /// データテーブル用の便マスター情報取得SQL作成
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用終了日時を過ぎた便を含むか</param>
        /// <param name="refferenceDate">基準日時</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTripsForDataTable(bool isBeforeApplicablePeriod, DateTime refferenceDate)
        {
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    FORMAT(CONVERT(DATETIME, TripHistories.day_shift_start_time), 'HH:mm'),
                    FORMAT(TripHistories.applicable_start_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(TripHistories.applicable_end_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(TripHistories.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    TripHistories.created_by,
                    FORMAT(TripHistories.updated_at, 'yyyy/MM/dd HH:mm:ss'),
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
            // 適用終了日時を過ぎた便を含まない場合
            if (!isBeforeApplicablePeriod)
            {
                string formatRefferenceDate = refferenceDate.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                    WHERE
                        TripHistories.applicable_end_datetime > '{formatRefferenceDate}'
                ";
            }

            sql += $@"
                    ORDER BY 
                        TripHistories.trip_id
                ";
            return sql;
        }

        /// <summary>
        /// データテーブル用の便マスター、便枝番マスター結合情報取得SQL作成
        /// </summary>
        /// <param name="refferenceDate">基準日時</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTripBranchesForDataTable(DateTime refferenceDate)
        {
            string formatRefferenceDate = refferenceDate.ToString("yyyy/MM/dd HH:mm:ss");
            var sql = $@"
                SELECT 
	                TripHistories.trip_id,
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    FORMAT(CONVERT(DATETIME, TripHistories.day_shift_start_time), 'HH:mm') AS day_shift_start_time,
                    FORMAT(TripHistories.applicable_start_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(TripHistories.applicable_end_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(TripHistories.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    TripHistories.created_by,
                    FORMAT(TripHistories.updated_at, 'yyyy/MM/dd HH:mm:ss'),
                    TripHistories.updated_by,
	                BranchNumbers.trip_branch_number_id,
	                FORMAT(CONVERT(DATETIME, BranchNumbers.arrival_scheduled_time), 'HH:mm') AS arrival_scheduled_time,
	                FORMAT(CONVERT(DATETIME, BranchNumbers.departure_scheduled_time), 'HH:mm'),
	                FORMAT(BranchNumbers.applicable_start_datetime, 'yyyy/MM/dd HH:mm:ss'),
	                FORMAT(BranchNumbers.applicable_end_datetime, 'yyyy/MM/dd HH:mm:ss'),
	                FORMAT(BranchNumbers.created_at, 'yyyy/MM/dd HH:mm:ss'),
	                BranchNumbers.created_by,
	                FORMAT(BranchNumbers.updated_at, 'yyyy/MM/dd HH:mm:ss'),
	                BranchNumbers.updated_by
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
                INNER JOIN
	                m_trip_branch_numbers as BranchNumbers
                ON
	                TripHistories.trip_id = BranchNumbers.trip_id
                WHERE
                    TripHistories.applicable_start_datetime < '{formatRefferenceDate}'
                AND
                    '{formatRefferenceDate}' < TripHistories.applicable_end_datetime
                AND
                    BranchNumbers.applicable_start_datetime < '{formatRefferenceDate}'
                AND
                    '{formatRefferenceDate}' < TripHistories.applicable_end_datetime
                ORDER BY
                    TripHistories.trip_id, BranchNumbers.arrival_scheduled_time
            ";
            return sql;
        }

        /// <summary>
        /// 便名称が重複している適用期間取得SQL作成
        /// </summary>
        /// <param name="model">調査対象の便情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectApplicablePeriodFromDuplicateMTripName(M_TripModel model)
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
        /// <param name="tripName">調査対象の便名称</param>
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
                    '{model.TruckID}',
                    '{model.DriverName}',
                    '1900/01/01 {model.RegistDayShiftStartTime}:00',
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
	                truck_id = {model.TruckID},
	                day_shift_start_time = '1900/01/01 {model.RegistDayShiftStartTime}:00',
	                applicable_start_datetime = '{model.ApplicableStartDateTime}',
	                applicable_end_datetime = '{model.ApplicableEndDateTime}',
                    updated_at = '{formatupdatedAt}',
                    updated_by = '{updatedBy}'
                WHERE trip_history_id = {model.TripHistoryID}
            ";
            return sql;
        }

        /// <summary>
        /// 車両IDで識別番号を取得するSQL作成
        /// </summary>
        /// <param name="truckId">車両ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectIdentifyNumberByTruckId(int truckID)
        {
            var sql = $@"
                    SELECT
                        identify_number               
                    FROM 
                        m_trucks
                    WHERE
                        truck_id = {truckID}
                        AND is_deleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 便履歴IDをもとに便データを取得するSQL
        /// </summary>
        /// <param name="tripHistoryId"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTripHistoryByTripHistoryId(int tripHistoryId)
        {
            var sql = $@"
                    SELECT 
                    Trips.trip_name,
                    TripHistories.driver_name,
                    Trucks.truck_id,
                    Trucks.truck_number,
                    Trucks.identify_number,
                    CONVERT(DATETIME, TripHistories.day_shift_start_time) AS day_shift_start_time,
                    TripHistories.applicable_start_datetime,
                    TripHistories.applicable_end_datetime
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
                WHERE 
                    TripHistories.trip_history_id = {tripHistoryId}
            ";
            return sql;
        }
    }
}
