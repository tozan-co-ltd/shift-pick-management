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
                    truck_number = '{model.TruckNumber}'
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
                    identify_number = '{model.IdentifyNumber}'
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
        public static string CreateSQLToInsertMTruck(M_TruckModel model, DateTime createdAt, string createdBy)
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
        public static string CreateSQLToUpdateMTruck(M_TruckModel model, DateTime updatedAt, string updatedBy)
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
        public static string CreateSQLToDeleteMTruck(int truckId, DateTime updatedAt, string updatedBy)
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

