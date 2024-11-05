using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Data;

/// <summary>
/// 車両マスターに関する関数
/// </summary>
namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_TruckConnectController
    {
        /// <summary>
        /// 車両情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_TruckModel> ConnectMTrucks(string sql)
        {
            // 戻り値
            List<M_TruckModel> strList = new();

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
                    strList = connection.Query<M_TruckModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 車両情報をデータテーブルとして取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static DataTable ConnectMTrucksToDataTable(string sql)
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
        /// 車両情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMTruck(M_TruckModel model, LoginUserModel loginUser)
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
                    string sql = CreateSQLToInsertMTruck(model, sysDate, loginUser.UserName);
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
        /// 車両情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int UpdateMTruck(M_TruckModel model, LoginUserModel loginUser)
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
                    string sql = CreateSQLToUpdateMTruck(model, sysDate, loginUser.UserName);
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
        /// 車両情報削除
        /// </summary>
        /// <param name="truckId">車両ID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMTruck(int truckId, LoginUserModel loginUser)
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
                    string sql = CreateSQLToDeleteMTruck(truckId, sysDate, loginUser.UserName);
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
        /// 車両マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrucks()
        {
            var sql = $@"
                SELECT 
	                truck_id
                    ,truck_number
                    ,identify_number
                    ,is_deleted
                    ,created_at
                    ,created_by
                    ,updated_at
                    ,updated_by
                FROM 
	                m_trucks
                WHERE 
                    is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// DataTable用の車両マスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMTrucksForDataTable()
        {
            var sql = $@"
                SELECT 
	                truck_id
                    ,truck_number
                    ,identify_number
                    ,is_deleted
                    ,FORMAT (created_at, 'yyyy/MM/dd HH:mm:ss')
                    ,created_by
                    ,FORMAT (updated_at, 'yyyy/MM/dd HH:mm:ss')
                    ,updated_by
                FROM 
	                m_trucks
                WHERE 
                    is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 重複識別番号取得SQL作成
        /// </summary>
        /// <param name="truckNumber">車両コード</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMTruckIdentifyNumber(M_TruckModel model)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_trucks
                WHERE
                    identify_number = {model.IdentifyNumber}
                    AND truck_id <> {model.TruckID}
                    AND is_deleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なる車両IDで重複車両番号情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMTruck(M_TruckModel model)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_trucks
                WHERE
                    truck_number = {model.TruckNumber}
                    AND truck_id <> {model.TruckID}
                    AND is_deleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なる車両IDで重複車両番号情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMTruckIdentifyNumber(M_TruckModel model)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_trucks
                WHERE
                    identify_number = {model.IdentifyNumber}
                    AND truck_id <> {model.TruckID}
                    AND is_deleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 車両マスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMTruck(M_TruckModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO m_trucks(
                    truck_number, 
                    identify_number,
                    created_at,
                    created_by,
                    updated_at,
                    updated_by
                )
                VALUES (
                    '{model.TruckNumber}',
                    '{model.IdentifyNumber}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 車両マスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMTruck(M_TruckModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_trucks
                SET 
                    truck_number = '{model.TruckNumber}',
                    identify_number = '{model.IdentifyNumber}',
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE
                    truck_id = {model.TruckID}
                    and is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 車両マスター削除SQL作成
        /// </summary>
        /// <param name="truckId"></param>
        /// <param name="updatedAt"></param>
        /// <param name="updatedBy"></param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMTruck(int truckId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_trucks
                SET 
                    is_deleted = 1,
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE 
                    truck_id = {truckId}
            ;";
            return sql;
        }
    }
}

