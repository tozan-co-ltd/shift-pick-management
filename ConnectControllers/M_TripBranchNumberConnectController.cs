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

        public static string CreateSQLToSelectMTripBranchNumbers(int tripId, bool isBeforeApplicablePeriod, DateTime? applicablePeriod)
        {
            var sql = $@"
                SELECT
	                trip_branch_number_id,
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
                sql += $@"
                AND applicable_end_datetime > '{applicablePeriod.Value.ToString("yyyy/MM/dd")}'
                ";
            }
            return sql;
        }

        public static string CreateSQLToSelectTripNameFromTripID(int tripID)
        {
            var sql = $@"
                SELECT trip_name
                FROM m_trips
                WHERE trip_id = {tripID}
            ";
            return sql;
        }
    }
}
