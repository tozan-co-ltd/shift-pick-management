using ai_truck_load_measurement.Commons;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using ai_truck_load_measurement.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoadRecordConnectController
    {
        /// <summary>
        /// 便実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<LoadRecordModel> ConnectTTripRecords(string sql)
        {
            // 戻り値
            List<LoadRecordModel> strList = new();

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
                    strList = connection.Query<LoadRecordModel>(sql).ToList();
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
        /// <returns></returns>
        public static DataTable ConnectTTripRecordToDataTable(string sql)
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
        /// 選択された便名称と便枝番からSQLの検索条件箇所を作成する
        /// </summary>
        /// <param name="models">選択された便名称と便枝番のリスト</param>
        /// <returns>SQL文</returns>
        public static string SelectedTripsSQL(List<LoadRecordModel> models)
        {
            var selectedTrips = "";
            for (int i = 0; i < models.Count; i++)
            {
                if (i != 0)
                {
                    selectedTrips += " OR ";
                }
                selectedTrips += @$"(trip_name = '{models[i].TripName}' AND trip_branch_seq = '{models[i].TripBranchSeq}')";
            }
            return selectedTrips;
        }

        /// <summary>
        /// 便名称取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="">データベース名</param>
        /// <returns></returns>
        public static List<SelectListItem> ConnectTTripRecordsForTripName(string sql)
        {
            // 戻り値
            List<SelectListItem> strList = new();

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
                    strList = connection.Query<SelectListItem>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便枝番リスト取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="">データベース名</param>
        /// <returns></returns>
        public static List<int> ConnectTTripRecordsForTripBranchSeq(string sql)
        {
            // 戻り値
            List<int> strList = new();

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
                    strList = connection.Query<int>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 便実績IDと到着か否かから訂正後荷量クラスを取得する
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="isArrived">到着か否か</param>
        /// <returns></returns>
        public static int GetAnnotationLoadClassByTripRecordIDAndIsArrived(int tripRecordID, bool isArrived)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
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
        ///「荷量の相違あり」で保存した値があるか
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="isArrived">到着か否か</param>
        public static bool IsSameAnnotationLoadsExist(int tripRecordID, bool isArrived)
        {
            // 戻り値
            var isAnnotationLoadsExist = false;

            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
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
        /// 荷量の相違あり情報更新
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int UpdateAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
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
        /// 荷量の相違あり情報登録
        /// </summary>
        /// <param name="tripRecordID">便実績ID</param>
        /// <param name="loadStatus">荷量クラス</param>
        /// <param name="isArrived">到着か否か</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertAnnotationLoads(int tripRecordID, int loadStatus, bool isArrived, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
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
        /// 便実績情報取得SQL
        /// </summary>
        /// <returns></returns>
        public static string CreatSQLToSelectTripRecord()
        {
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
                TripRecords.station_id = Stations.station_id";
            return sql;
        }

        /// <summary>
        /// 便名称取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripNameFromPeriod(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT DISTINCT
	                trip_name ,
	                trip_branch_seq
                FROM t_trip_records AS TripRecords
                INNER JOIN
	                m_trip_histories AS TripHistories
                ON 
	                TripRecords.trip_id = TripHistories.trip_id
                WHERE work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
                AND trip_name IS NOT NULL
            ";
            if(checkedDepos.Count != 0 )
            {
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
            }
            return sql;
        }

        /// <summary>
        /// 便名称から便枝番取得SQL
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripBranchSeqFromTripName(string tripName, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT DISTINCT
                    trip_branch_seq
                FROM t_trip_records
                WHERE trip_name = '{tripName}'
                AND work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
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
        /// <param name="updatedBy">更新者</param>
        /// <param name="updatedAt">更新日時</param>
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

        /// <summary>
        /// デポIDリストからデポ名リストを取得するSQLを生成する
        /// </summary>
        /// <param name="depoIDs">デポIDリスト</param>
        /// <returns></returns>
        public static string CreateSQLToSelectDepoNameFromDepoID(List<string> depoIDs)
        {
            var sql = $@"
                SELECT
                    name AS depo_name
                FROM
                    m_depos
            ";
            if (depoIDs.Count != 0)
            {
                sql +="WHERE ";
                for (int i = 0; i < depoIDs.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += " OR ";
                    }
                    sql += $@"depo_id = {depoIDs[i]}";
                }
            }
            
            return sql;
        }
    }
}
