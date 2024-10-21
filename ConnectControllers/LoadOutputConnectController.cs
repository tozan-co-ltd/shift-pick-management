using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using System;
using System.Data;
using NPOI.SS.Formula.Functions;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadOutputConnectController
    {
        /// <summary>
        /// 便実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<LoadOutputModel> ConnectTTripRecords(string sql, string databaseName)
        {
            // 戻り値
            List<LoadOutputModel> strList = new();

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
                    strList = connection.Query<LoadOutputModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便実績情報をデータテーブルとして取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static DataTable ConnectTTripRecordToDataTable(string sql, string databaseName)
        {
            // 戻り値
            DataTable dataTable = new DataTable();

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
        ///「荷量の相違あり」で保存した値があるか
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="isArrived">到着か否か</param>
        public static bool IsSameAnnotationLoadsExist(int tripRecordID, bool isArrived)
        {
            // 戻り値
            var isAnnotationLoadsExist = false;

            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString("AI-truck-load-measurement_test");
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                try
                {
                    string sql = CreateSQLToSelectAnnotationLoadClassByTripRecordIDAndIsArrived(tripRecordID, isArrived);
                    // 同じ便実績IDかつ到着か否かが一致するデータが存在する場合、値が代入される
                    var reader = connection.ExecuteScalar(sql);
                    if (reader != null)
                    {
                        isAnnotationLoadsExist = true;
                    }
                    return isAnnotationLoadsExist;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 荷量の相違あり情報登録
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived, LoginUserModel loginUser, string databaseName)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToInsertAnnotaionLoads(tripRecordID, loadStatus, loginUser.UserName, sysDate, isArrived);
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
        /// 荷量の相違あり情報更新
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int UpdateAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived, LoginUserModel loginUser, string databaseName)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToUpdateAnnotationLoads(tripRecordID, loadStatus, loginUser.UserName, sysDate, isArrived);
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
        /// 便実績IDと到着か否かから訂正後荷量クラスを取得する
        /// </summary>
        /// <param name="tripRecordID"></param>
        /// <param name="isArrived"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static int GetAnnotationLoadClassByTripRecordIDAndIsArrived(int  tripRecordID, bool isArrived, string databaseName)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                try
                {
                    string sql = CreateSQLToSelectAnnotationLoadClassByTripRecordIDAndIsArrived(tripRecordID, isArrived);
                    var annotationLoadClass = Convert.ToInt32(connection.ExecuteScalar(sql));
                    return annotationLoadClass;
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }

        /// <summary>
        /// 便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecord(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDeference)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
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
                ";
            if (isOnlyHasAmountDeference)
            {
                sql += $@"
                FROM t_annotation_loads
                INNER JOIN t_trip_records
                ON t_annotation_loads.trip_record_id = t_trip_records.trip_record_id
                ";
            }
            else
            {
                sql += $@"
                FROM t_trip_records";
            }
            sql += $@"
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ORDER BY arrived_at";
            return sql;
        }

        /// <summary>
        /// データベース用便実績情報取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripRecordForDataTable(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDeference)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
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
	                FORMAT(work_day, 'yyyy/MM/dd'),
	                FORMAT(arrived_at, 'yyyy/MM/dd HH:mm'),
	                FORMAT(departed_at, 'yyyy/MM/dd HH:mm'),
	                arrival_load_class,
	                departure_load_class,
	                arrival_load_img_path,
	                departure_load_img_path
                ";
            if (isOnlyHasAmountDeference)
            {
                sql += $@"
                FROM t_annotation_loads
                INNER JOIN t_trip_records
                ON t_annotation_loads.trip_record_id = t_trip_records.trip_record_id
                ";
            }
            else
            {
                sql += $@"
                FROM t_trip_records";
            }
            sql += $@"
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                ORDER BY arrived_at, trip_name, trip_branch_seq
";
            return sql;
        }

        /// <summary>
        /// 便実績IDと到着、出発の属性が同じデータを取得するSQL
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public static string CreateSQLToSelectAnnotationLoadClassByTripRecordIDAndIsArrived(int tripRecordID, bool isArrived)
        {
            var arrivalOrDeparture = GetArrivalOrDeparture(isArrived);
            var sql = $@"
                SELECT annotation_load_class
                FROM t_annotation_loads
                WHERE 
                    trip_record_id = '{tripRecordID}'
                AND
                    arrival_departure_class = '{arrivalOrDeparture}'
            ";
            return sql;
        }

        /// <summary>
        /// 荷量の相違ありデータ登録SQL
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="createdBy">登録者</param>
        /// <param name="createdAt">登録日時</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public static string CreateSQLToInsertAnnotaionLoads(int tripRecordID, int loadStatus, string createdBy, DateTime createdAt, bool isArrived)
        {
            var arrivalOrDeparture = GetArrivalOrDeparture(isArrived);
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm");
            var sql = $@"
                INSERT INTO t_annotation_loads(
	                trip_record_id,
	                arrival_departure_class,
                    annotation_load_class,
	                created_at,
	                created_by,
	                updated_at,
	                updated_by
                )
                VALUES(
                    '{tripRecordID}',
                    '{arrivalOrDeparture}',
                    '{loadStatus}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                )            
            ";
            return sql;
        }

        /// <summary>
        /// 荷量の相違ありデータ更新SQL
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="createdBy">登録者</param>
        /// <param name="createdAt">登録日時</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public static string CreateSQLToUpdateAnnotationLoads(int tripRecordID, int loadStatus, string updatedBy, DateTime updatedAt, bool isArrived)
        {
            var arrivalOrDeparture = GetArrivalOrDeparture(isArrived);
            string formatUpdatedAt = updatedAt.ToString("yyyy/MM/dd HH:mm");
            var sql = $@"
                UPDATE t_annotation_loads
                SET
                    annotation_load_class = '{loadStatus}',
                    updated_at = '{formatUpdatedAt}',
                    updated_by = '{updatedBy}'
                WHERE
                    trip_record_id = '{tripRecordID}'
                AND
                    arrival_departure_class = '{arrivalOrDeparture}'
            ";
            return sql;
        }

        /// <summary>
        /// 荷量の相違ありテーブルの到着出発クラスに保存する値
        /// </summary>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        private static string GetArrivalOrDeparture(bool isArrived)
        {
            var arrivalOrDeparture = string.Empty;
            if (isArrived)
            {
                arrivalOrDeparture = "arrival";
            }
            else
            {
                arrivalOrDeparture = "departure";
            }
            return arrivalOrDeparture;
        }
    }
}
