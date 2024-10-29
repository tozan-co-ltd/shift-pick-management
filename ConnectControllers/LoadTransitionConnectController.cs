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
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<LoadTransitionModel> ConnectTTripRecords(string sql, string databaseName)
        {
            // 戻り値
            List<LoadTransitionModel> strList = new();

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
        /// 便名称取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<SelectListItem> ConnectTTripRecordsForTripName(string sql, string databaseName)
        {
            // 戻り値
            List<SelectListItem> strList = new();

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
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<int> ConnectTTripRecordsForTripBranchSeq(string sql, string databaseName)
        {
            // 戻り値
            List<int> strList = new();

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
        /// 便名称取得SQL
        /// </summary>
        /// <param name="startOfPeriod">期間開始日</param>
        /// <param name="endOfPeriod">期間終了日</param>
        /// <param name="isOnlyHasAmountDeference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripNameFromPeriod()
        {
            var sql = $@"
                SELECT DISTINCT
	                trip_name AS Value,
	                trip_name AS Text
                FROM t_trip_records
            ";
            return sql;
        }

        /// <summary>
        /// 便名称から便枝番取得SQL
        /// </summary>
        /// <param name="tripName">便名称</param>
        /// <returns></returns>
        public static string CreateSQLToSelectTripBranchSeqFromTripName(string tripName)
        {
            var sql = $@"
                SELECT DISTINCT
                    trip_branch_seq
                FROM t_trip_records
                WHERE trip_name = '{tripName}'
            ";
            return sql;
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
	                arrival_load_class,
	                departure_load_class
                FROM t_trip_records
                WHERE trip_name = '{tripName}'
                AND trip_branch_seq = '{tripBranchSeq}'
                AND work_day BETWEEN '{startOfPeriod}' AND '{endOfPeriod}'
            ";
            return sql;
        }
    }
}

