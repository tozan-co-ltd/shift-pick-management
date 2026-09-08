using Dapper;
using shift_pick_management.Commons;
using shift_pick_management.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace shift_pick_management.ConnectControllers
{
    public class M_NotificationConnectController 
    {

        /// <summary>
        /// 通知マスターと通知ユーザー登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMNotificationAndRNotificationUser(M_NotificationModel model, LoginUserModel loginUser)
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
                        // 通知マスター登録
                        var notificationId = InsertMNotification(model, loginUser.UserName, connection, command);

                        // 通知ユーザー登録
                        insertedCount = InsertRNotificationUser(model, notificationId, loginUser.UserName, connection, command);

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
        /// 通知マスター登録
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns>通知ID</returns>
        private static int InsertMNotification(M_NotificationModel model, string userName, SqlConnection connection, SqlCommand command)
        {
            string notificationSql = CreateSQLToInsertMNotification(model, userName);
            var notificationId = Int32.Parse(connection.ExecuteScalar(notificationSql, new { }, command.Transaction).ToString());
            return notificationId;
        }

        /// <summary>
        /// 通知ユーザー登録
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns></returns>
        private static int InsertRNotificationUser(M_NotificationModel model, int notificationId, string userName, SqlConnection connection, SqlCommand command)
        {
            // 通知ユーザーのAD名とユーザーIDを取得してモデルリスト化する
            var notificationUserList = GetNotificationUserList(model);
            int insertedCount = 0;
            // 各通知ユーザーを登録
            foreach (var notificationUser in notificationUserList)
            {
                var sql = CreateSQLToInsertRNotificationUsers(notificationUser, notificationId, userName);
                insertedCount += connection.Execute(sql, new { }, command.Transaction);
            }
            return insertedCount;
        }

        /// <summary>
        /// 通知マスターと通知ユーザー更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="loginUser"></param>
        /// <returns></returns>
        public static int UpdateMNotificationAndRNotificationUser(M_NotificationModel model, LoginUserModel loginUser)
        {
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
                    int insertedCount = 0;

                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    try
                    {
                        // 通知マスター登録
                        var notificationId = UpdateMNotification(model, loginUser.UserName, connection, command);

                        // 通知ユーザー登録
                        insertedCount = UpdateRNotificationUser(model, loginUser.UserName, connection, command);

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
        /// 通知マスター更新
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns></returns>
        public static int UpdateMNotification(M_NotificationModel model, string userName, SqlConnection connection, SqlCommand command)
        {
            string notificationSql = CreateSQLToUpdateMNotification(model, userName);
            var notificationId = connection.Execute(notificationSql, new { }, command.Transaction);
            return notificationId;
        }


        /// <summary>
        /// 通知ユーザー更新
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns></returns>
        private static int UpdateRNotificationUser(M_NotificationModel model, string userName, SqlConnection connection, SqlCommand command)
        {
            // 通知ユーザーの削除フラグを一旦全て1に
            var deleteSql = CreateSQLToDeleteRNotificationUser(model.NotificationID);
            var deleteCount = connection.Execute(deleteSql, new { }, command.Transaction);

            // 通知ユーザーのAD名とユーザーIDを取得してモデルリスト化する
            var notificationUserList = GetNotificationUserList(model);
            int insertedCount = 0;

            // 各通知ユーザーを登録
            foreach (var notificationUser in notificationUserList)
            {
                var sql = CreateSQLToUpdateRNotificationUsers(model.NotificationID, notificationUser.UserID, userName);
                insertedCount += connection.Execute(sql, new { }, command.Transaction);
            }
            return insertedCount;
        }

        /// <summary>
        /// 通知情報削除
        /// </summary>
        /// <param name="truckId">車両ID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMNotificationAndRNotificationUser(int notificationID, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                using (var command = connection.CreateCommand())
                {
                    // トランザクションの開始
                    command.Transaction = connection.BeginTransaction();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    var notificationDeletedCount = 0;
                    try
                    {
                        // 通知マスター削除
                        notificationDeletedCount += DeleteMNotification(notificationID, loginUser, connection, command);

                        // 通知ユーザー削除
                        notificationDeletedCount += DeleteRNotificationUser(notificationID, connection, command);

                    }
                    catch (Exception)
                    {
                        command.Transaction.Rollback();
                        throw;
                    }
                    command.Transaction.Commit();
                    return notificationDeletedCount;
                }
            }
        }

        /// <summary>
        /// 通知情報削除
        /// </summary>
        /// <param name="notificationID">通知ID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <param name="connection">SQLConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns>更新件数</returns>
        public static int DeleteMNotification(int notificationID, LoginUserModel loginUser, SqlConnection connection, SqlCommand command)
        {
            string sql = CreateSQLToDeleteMNotification(notificationID, loginUser.UserName);
            var count = connection.Execute(sql, new { }, command.Transaction);

            return count;
        }

        /// <summary>
        /// 通知ユーザー情報削除
        /// </summary>
        /// <param name="notificationID">通知ID</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="command">SqlCommand</param>
        /// <returns></returns>
        public static int DeleteRNotificationUser(int notificationID, SqlConnection connection, SqlCommand command)
        {
            string sql = CreateSQLToDeleteRNotificationUser(notificationID);
            var count = connection.Execute(sql, new { }, command.Transaction);

            return count;
        }

        /// <summary>
        /// 通知ユーザーのAD名とユーザーIDを取得してモデルリスト化する
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static List<R_NotificationUserModel> GetNotificationUserList(M_NotificationModel model)
        {
            List<R_NotificationUserModel> notificationUserList = new();
            var notificationUserStringList = model.NotificationUsersView;

            if (notificationUserStringList == null)
                return notificationUserList;

            foreach(var notificationUserString in notificationUserStringList)
            {
                var adNameAndUserId = notificationUserString.Split('/');
                R_NotificationUserModel notificationUser = new R_NotificationUserModel()
                {
                    ADName = adNameAndUserId[0],
                    UserID = Int32.Parse(adNameAndUserId[1]),
                };
                notificationUserList.Add(notificationUser);
            }

            return notificationUserList;
        }

        /// <summary>
        /// 全通知情報取得SQL作成
        /// </summary>
        /// <param name="depoList">デポリスト</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMNotificationsAll()
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                SELECT 
                    Notifications.notification_id,
                    Notifications.trip_id,
                    Trips.trip_name,
                    Notifications.trip_branch_seq,
                    Depos.depo_id,
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
                    Notifications.is_deleted <> 1
                AND
                    TripHistories.applicable_end_datetime > '{today.ToString("yyyy/MM/dd HH:mm")}'
                ORDER BY 
                    Notifications.notification_id, Notifications.notification_start_datetime
            ";
            return sql;
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
                    Notifications.trip_id,
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
                AND
                    Notifications.is_deleted <> 1
                AND
                    TripHistories.applicable_end_datetime > '{today.ToString("yyyy/MM/dd HH:mm")}'
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
        /// 通知ユーザー情報取得SQL作成
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRNotificationUsers(int notificationId)
        {
            var sql = $@"
                SELECT 
                    NotificationUsers.notification_user_id
                    ,NotificationUsers.notification_id
                    ,NotificationUsers.user_id
                    ,Users.ad_name
                    ,NotificationUsers.is_deleted
                    ,NotificationUsers.created_at
                    ,NotificationUsers.created_by
                FROM 
                    r_notification_users AS NotificationUsers
                INNER JOIN
                    m_users AS Users
                ON
                    NotificationUsers.user_id = Users.user_id
                WHERE
                    notification_id = {notificationId}
                AND
                    NotificationUsers.is_deleted <> 1
            ";
            return sql;
        }

        /// <summary>
        /// デポマスター情報取得SQL作成
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMDepos()
        {
            var sql = $@"
                SELECT
                    depo_id
                    ,name
                FROM
                    m_depos
            ";
            return sql;
        }

        /// <summary>
        /// 便マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrips(int depoID)
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
                AND
                    TripHistories.depo_id = '{depoID}'
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
        public static string CreateSQLToSelectMTripBranchNumberFromTripBranchNumberID(int tripId, int tripBranchNumberId)
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
                AND trip_branch_number_id = '{tripBranchNumberId}'
                AND applicable_end_datetime > '{today}'
            ";
            return sql;
        }

        /// <summary>
        /// 通知情報登録SQL作成
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertMNotification(M_NotificationModel model, string createdBy)
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

        /// <summary>
        /// 通知ユーザー情報登録SQL作成
        /// </summary>
        /// <param name="model">通知ユーザーモデル</param>
        /// <param name="notificationId">通知ID</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertRNotificationUsers(R_NotificationUserModel model, int notificationId, string createdBy)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                INSERT INTO r_notification_users
                           ([notification_id]
                           ,[user_id]
                           ,[is_deleted]
                           ,[created_at]
                           ,[created_by])
                     VALUES
                           ({notificationId}
                           ,{model.UserID}
                           ,'false'
                           ,'{today.ToString("yyyy-MM-dd HH:mm:ss")}'
                           ,'{createdBy}')
            ";
            return sql;
        }

        /// <summary>
        /// 通知情報更新SQL作成
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns></returns>
        public static string CreateSQLToUpdateMNotification(M_NotificationModel model, string updatedBy)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                UPDATE m_notifications
                   SET [trip_id] = '{model.TripID}'
                      ,[trip_branch_seq] = '{model.TripBranchSeq}'
                      ,[arrival_lower_load_class] = '{model.ArrivalLowerLoadClass}'
                      ,[departure_lower_load_class] = '{model.DepartureLowerLoadClass}'
                      ,[notification_start_datetime] = '{model.NotificationStartDateTime.ToString("yyyy-MM-dd HH:mm")}'
                      ,[notification_end_datetime] = '{model.NotificationEndDateTime.ToString("yyyy-MM-dd HH:mm")}'
                      ,[updated_at] = '{today.ToString("yyyy-MM-dd HH:mm:ss")}'
                      ,[updated_by] = '{updatedBy}'
                 WHERE 
                    notification_id = {model.NotificationID}
            ";
            return sql;
        }

        /// <summary>
        /// 通知ユーザー情報更新SQL作成
        /// </summary>
        /// <param name="notificationID">通知ID</param>
        /// <param name="userID">ユーザーID</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns></returns>
        public static string CreateSQLToUpdateRNotificationUsers(int notificationID, int userID,  string updatedBy)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                MERGE INTO r_notification_users AS NotificationUsers
                 USING
	                (SELECT
		                {notificationID} AS notification_id,
		                {userID} AS user_id,
		                0 AS is_deleted,
		                '{updatedBy}' AS created_by,
		                '{today.ToString("yyyy-MM-dd HH:mm:ss")}' AS created_at
	                ) AS US
                ON NotificationUsers.notification_id = US.notification_id
	                AND NotificationUsers.user_id = US.user_id
                WHEN MATCHED THEN
                UPDATE SET
	                NotificationUsers.is_deleted = US.is_deleted
                WHEN NOT MATCHED THEN
                INSERT
                (
	                notification_id
	                ,user_id
	                ,is_deleted
	                ,created_at
	                ,created_by
                )
                VALUES
                (
	                US.notification_id
	                ,US.user_id
	                ,US.is_deleted
	                ,US.created_at
	                ,US.created_by
                );
            ";
            return sql;
        }

        /// <summary>
        /// 通知情報削除SQL作成
        /// </summary>
        /// <param name="notificationID"></param>
        /// <param name="updatedBy"></param>
        /// <returns></returns>
        public static string CreateSQLToDeleteMNotification(int notificationID, string updatedBy)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                UPDATE m_notifications
                SET is_deleted = 1
                    ,updated_at = '{today.ToString("yyyy-MM-dd HH:mm:ss")}'
                    ,updated_by = '{updatedBy}'
    
                WHERE
                    notification_id = {notificationID}
            ";
            return sql;
        }

        /// <summary>
        /// 通知ユーザー情報削除SQL作成
        /// </summary>
        /// <param name="notificationID">通知ID</param>
        /// <returns></returns>
        public static string CreateSQLToDeleteRNotificationUser(int notificationID)
        {
            DateTime today = DateTime.Now;
            var sql = $@"
                UPDATE r_notification_users
                SET is_deleted = 1
                WHERE
                    notification_id = {notificationID}
            ";
            return sql;
        }

        /// <summary>
        /// データテーブル用の通知マスター情報取得SQL作成
        /// </summary>
        /// <param name="isBeforeNotificationPeriod">通知終了日時を過ぎた便を含むか</param>
        /// <param name="checkedDepos">チェックされたデポリスト</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMNotificationsForDataTable(bool isBeforeNotificationPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT 
	                Notifications.notification_id,
                    Trips.trip_name,
                    Notifications.trip_branch_seq,
	                Depos.name AS depo_name,
                    Notifications.arrival_lower_load_class,
                    Notifications.departure_lower_load_class,
                    Notifications.is_deleted,
                    FORMAT(Notifications.notification_start_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(Notifications.notification_end_datetime, 'yyyy/MM/dd HH:mm:ss'),
                    FORMAT(Notifications.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    Notifications.created_by,
                    FORMAT(Notifications.updated_at, 'yyyy/MM/dd HH:mm:ss'),
                    Notifications.updated_by
                FROM 
	                m_notifications AS Notifications
                INNER JOIN
                    m_trips AS Trips
                ON
                    Notifications.trip_id = Trips.trip_id
                INNER JOIN
                    m_trip_histories AS TripHistories
                ON 
                    Notifications.trip_id = TripHistories.trip_id
                INNER JOIN 
	                m_depos as Depos
                ON
	                TripHistories.depo_id = Depos.depo_id
                WHERE
                    {CommonConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            // 適用終了日時を過ぎた便を含まない場合
            if (!isBeforeNotificationPeriod)
            {
                string formatRefferenceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                    AND
                        Notifications.notification_end_datetime > '{formatRefferenceDate}'
                ";
            }

            sql += $@"
                    ORDER BY 
                        Notifications.notification_id
                ";
            return sql;
        }

        /// <summary>
        /// データテーブル用の通知ユーザー情報取得SQL作成
        /// </summary>
        /// <param name="isBeforeNotificationPeriod">通知終了日時を過ぎた便を含むか</param>
        /// <param name="checkedDepos">チェックされたデポリスト</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRNotificationUsersForDataTable(bool isBeforeNotificationPeriod, List<string> checkedDepos)
        {
            var sql = $@"
                SELECT 
                    NotificationUsers.notification_user_id,
	                NotificationUsers.notification_id,
                    Users.ad_name,
                    NotificationUsers.is_deleted,
                    FORMAT(NotificationUsers.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    NotificationUsers.created_by
                FROM
                    r_notification_users AS NotificationUsers
                INNER JOIN
	                m_notifications AS Notifications
                ON
                    NotificationUsers.notification_id = notifications.notification_id
                INNER JOIN
                    m_trip_histories AS TripHistories
                ON 
                    Notifications.trip_id = TripHistories.trip_id
                INNER JOIN
                    m_users AS Users
                ON
                    NotificationUsers.user_id = Users.user_id
                WHERE
                    {CommonConnectController.SQLOfCheckedDepos(checkedDepos)}
            ";
            // 通知終了日時を過ぎた便を含まない場合
            if (!isBeforeNotificationPeriod)
            {
                string formatRefferenceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                    AND
                        Notifications.notification_end_datetime > '{formatRefferenceDate}'
                ";
            }

            sql += $@"
                    ORDER BY 
                        NotificationUsers.notification_user_id
                ";
            return sql;
        }
    }
}
