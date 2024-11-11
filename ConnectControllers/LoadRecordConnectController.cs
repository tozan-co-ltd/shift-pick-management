using ai_truck_load_measurement.Commons;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using ai_truck_load_measurement.Models;
using Dapper;

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
                FROM t_trip_records";
            return sql;
        }
    }
}
