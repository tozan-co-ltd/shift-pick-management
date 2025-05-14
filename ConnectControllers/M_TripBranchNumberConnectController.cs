using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_TripBranchNumberConnectController 
    {
        /// <summary>
        /// 便情報取得
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
        /// 便IDから便名称を取得
        /// </summary>
        /// <param name="tripID">便ID</param>
        /// <returns></returns>
        public static string ConnectMTripForTripNameFromTripID(int tripID)
        {
            var tripName = "";
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
                    var sql = CreateSQLToSelectTripNameFromTripID(tripID);
                    var strList = connection.Query<M_TripBranchNumberModel>(sql).ToList();
                    tripName = strList[0].TripName;
                }
                return tripName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便枝番情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMTripBranchNumber(M_TripBranchNumberModel model, LoginUserModel loginUser)
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
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToInsertMTripBranchNumber(model, sysDate, loginUser.UserName);
                    var insertedCount = connection.Execute(sql);
                    return insertedCount;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 便枝番情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int UpdateMTripBranchNumber(M_TripBranchNumberModel model, LoginUserModel loginUser)
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
                    DateTime sysDate = DateTime.Now;

                    // 便枝番テーブル更新
                    string sql = CreateSQLToUpdateMTripBranchNumber(model, sysDate, loginUser.UserName);
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
        /// 便枝番情報取得SQL作成
        /// </summary>
        /// <param name="tripId">便ID</param>
        /// <param name="isBeforeApplicablePeriod">適用期間を過ぎた便を表示するか</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMTripBranchNumbers(int tripId, bool isBeforeApplicablePeriod)
        {
            var sql = $@"
                SELECT
	                trip_branch_number_id,
                    tag,
	                CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
	                CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                applicable_start_datetime,
	                applicable_end_datetime,
	                updated_at,
	                updated_by
                FROM m_trip_branch_numbers
                WHERE trip_id = '{tripId}'
            ";
            if (!isBeforeApplicablePeriod)
            {
                DateTime today = DateTime.Now;
                string formatToday = today.ToString("yyyy/MM/dd HH:mm:ss");
                sql += $@"
                AND applicable_end_datetime > '{formatToday}'
                ";
            }
            return sql;
        }

        /// <summary>
        /// 基準日時以降の便枝番情報を取得するSQL作成
        /// </summary>
        /// <param name="tripId">便ID</param>
        /// <param name="refferenceDate">基準日時</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMTripBranchNumbersWithBranceSeq(int tripId, DateTime refferenceDate)
        {
            var sql = $@"
                SELECT
                    trip_branch_number_id,
                    tag,
                    CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
                    CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
                    BranchNumbers.applicable_start_datetime,
                    BranchNumbers.applicable_end_datetime,
                    BranchNumbers.updated_at,
                    BranchNumbers.updated_by,
	                CONVERT(DATETIME, day_shift_start_time) AS day_shift_start_time
                FROM m_trip_branch_numbers AS BranchNumbers
                INNER JOIN m_trip_histories AS TripHistories
                ON BranchNumbers.trip_id = TripHistories.trip_id
                WHERE BranchNumbers.trip_id = {tripId}
                AND BranchNumbers.applicable_end_datetime > '{refferenceDate.ToString("yyyy/MM/dd HH:mm:ss")}'
                AND BranchNumbers.applicable_start_datetime < '{refferenceDate.ToString("yyyy/MM/dd HH:mm:ss")}'
                ORDER BY arrival_scheduled_time
            ";
            return sql;
        }

        /// <summary>
        /// 便IDから便名称を取得するSQL作成
        /// </summary>
        /// <param name="tripID">便ID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripNameFromTripID(int tripID)
        {
            var sql = $@"
                SELECT trip_name
                FROM m_trips
                WHERE trip_id = {tripID}
            ";
            return sql;
        }

        /// <summary>
        /// 便枝番情報登録SQL作成
        /// </summary>
        /// <param name="model">便枝番情報</param>
        /// <param name="createdAt">作成日時</param>
        /// <param name="createdBy">作成者</param>
        /// <returns></returns>
        public static string CreateSQLToInsertMTripBranchNumber(M_TripBranchNumberModel model, DateTime createdAt, string createdBy)
        {
            var formatCreatedAt =createdAt.ToString("yyyy/MM/dd HH:mm:ss");
            var sql = $@"
                INSERT INTO m_trip_branch_numbers(
                    trip_id,
                    tag,
                    arrival_scheduled_time,
                    departure_scheduled_time,
                    applicable_start_datetime,
                    applicable_end_datetime,
                    created_at,
                    created_by,
                    updated_at,
                    updated_by
                )
                VALUES (
                    '{model.TripID}',
                    '{model.Tag}',
                    '1900/01/01 {model.RegistArrivalScheduledTime}:00',
                    '1900/01/01 {model.RegistDepartureScheduledTime}:00',
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
        /// 便枝番テーブル更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">更新日時</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMTripBranchNumber(M_TripBranchNumberModel model, DateTime updatedAt, string updatedBy)
        {
            string formatupdatedAt = updatedAt.ToString("yyyy/MM/dd HH:mm:ss");
            var sql = $@"
                UPDATE m_trip_branch_numbers
                SET 
                    trip_id = '{model.TripID}',
	                arrival_scheduled_time = '1900/01/01 {model.RegistArrivalScheduledTime}:00',
	                departure_scheduled_time = '1900/01/01 {model.RegistDepartureScheduledTime}:00',
	                applicable_start_datetime = '{model.ApplicableStartDateTime}',
	                applicable_end_datetime = '{model.ApplicableEndDateTime}',
                    updated_at = '{formatupdatedAt}',
                    updated_by = '{updatedBy}'
                WHERE trip_branch_number_id = {model.TripBranchNumberID}
            ";
            return sql;
        }

        /// <summary>
        /// 同じ便IDの適用期間と出発、到着予定時間を取得するSQL作成
        /// </summary>
        /// <param name="model">参照する便枝番情報</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTimesFromDuplicateTripID(M_TripBranchNumberModel model)
        {
            var sql = $@"
                SELECT 
                    CONVERT(DATETIME, arrival_scheduled_time) AS arrival_scheduled_time,
                    CONVERT(DATETIME, departure_scheduled_time) AS departure_scheduled_time,
	                applicable_start_datetime,
	                applicable_end_datetime
                FROM m_trip_branch_numbers
                WHERE
	                trip_id = '{model.TripID}'
                AND trip_branch_number_id <> {model.TripBranchNumberID}
            ";

            return sql;
        }
    }
}
