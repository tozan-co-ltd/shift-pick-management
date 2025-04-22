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
        /// 通知情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_NotificationModel> ConnectMNotifications(string sql)
        {
            // 戻り値
            List<M_NotificationModel> strList = new();

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
                    strList = connection.Query<M_NotificationModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

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
        /// 便情報と便履歴情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMNotification(M_NotificationModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // トランザクションの開始
                    command.Transaction = connection.BeginTransaction();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    int insertedCount;

                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    try
                    {
                        string sql =CreateSQLToInsetMNotification(model, loginUser.UserName);
                        insertedCount = connection.Execute(sql, new { }, command.Transaction);
                    }
                    catch (Exception)
                    {
                        command.Transaction.Rollback();
                        throw;
                    }
                    command.Transaction.Commit();
                    return insertedCount;
                }
            }
        }

        /// <summary>
        /// 通知情報取得SQL作成
        /// </summary>
        /// <param name="depoList">デポリスト</param>
        /// <returns></returns>
       public static string CreateSQLToSelectMNotifications(bool isBeforeNotificationPeriod, List<string> checkedDepos)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                SELECT 
                    Notifications.notification_id,
                    Trips.trip_name,
                    Notifications.trip_branch_seq,
	                Depos.name AS depo_name,
                    Notifications.arrival_lower_load_class,
                    Notifications.departure_lower_load_class,
                    Notifications.notification_start_datetime,
                    Notifications.notification_end_datetime,
                    Notifications.updated_by,
                    Notifications.updated_at
                FROM 
                    m_notifications as Notifications
                INNER JOIN
	                m_trips AS Trips
                ON
	                Notifications.trip_id = Trips.trip_id
                INNER JOIN
                    m_trip_histories as TripHistories
                ON 
                    Notifications.trip_id = TripHistories.trip_id
                INNER JOIN 
	                m_depos as Depos
                ON
	                TripHistories.depo_id = Depos.depo_id
                WHERE
                    {CommonConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            if (!isBeforeNotificationPeriod)
            {
                sql += $@"
                AND
                    Notifications.notification_end_datetime > '{today}'
                ";
            }
                
            sql += $@"
                ORDER BY 
                    Notifications.notification_id, Notifications.notification_start_datetime
            ";
            return sql;
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

        /// <summary>
        /// 通知情報登録SQL作成
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="createdBy">作成者</param>
        /// <returns></returns>
        public static string CreateSQLToInsetMNotification(M_NotificationModel model, string createdBy)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                INSERT INTO m_notifications
                           ([trip_id]
                           ,[trip_branch_seq]
                           ,[arrival_lower_load_class]
                           ,[departure_lower_load_class]
                           ,[notification_start_datetime]
                           ,[notification_end_datetime]
                           ,[created_at]
                           ,[created_by]
                           ,[updated_at]
                           ,[updated_by]
                           ,[is_deleted])
                     VALUES
                           ('{model.TripID}'
                           ,'{model.TripBranchSeq}'
                           ,'{model.ArrivalLowerLoadClass}'
                           ,'{model.DepartureLowerLoadClass}'
                           ,'{model.NotificationStartDateTime}'
                           ,'{model.NotificationEndDateTime}'
                           ,'{today.ToString("yyyy-MM-dd HH:mm:ss")}'
                           ,'{createdBy}'
                           ,'{today.ToString("yyyy-MM-dd HH:mm:ss")}'
                           ,'{createdBy}'
                           ,'false')
                    SELECT SCOPE_IDENTITY();
            ";
            return sql;
        }
    }
}
