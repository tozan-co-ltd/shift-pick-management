using Dapper;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 出荷実績テーブルに関する関数
    /// </summary>
    public static class D_ShipmentConnectController
    {
        /// <summary>
        /// 出荷実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_ShipmentModel> ConnectDShipments(string sql, string databaseName)
        {
            // 戻り値
            List<D_ShipmentModel> strList = new();

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

                    strList = connection.Query<D_ShipmentModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 出荷実績情報取得SQL作成
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="deliveryId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDShipments(int shipmentScheduleId, int depoId, int deliveryId)
        {
          
            var sql = $@"
                SELECT
                    schedule.DeliveryDate
                    ,schedule.DeliveryTimeClass
                    ,schedule.DeliveryProductNumber
					,schedule.DeliveryProductAbbreviation
                    ,company.CompanyID as DeliveryID
                    ,company.CompanyName as DeliveryName
                    ,depo.DepoID
                    ,depo.DepoName

                    ,shipment.ShipmentID
                    ,shipment.ShipmentScheduleID
                    ,shipment.ScanResultID
                    ,shipment.ShipmentDatetime
                    ,shipment.KanbanSerialNumber
                    ,shipment.SupplierProductNumber
                    ,shipment.LotNumber
                    ,shipment.MainProductKey
                    ,shipment.FirstSubProductKey
                    ,shipment.SecondSubProductKey
                    ,shipment.NumberOfBoxes
                    ,shipment.Quantity
                    --,shipment.ScanedAt
                    ,shipment.CreatedAt
                    ,shipment.CreatedBy

                    ,scanResult.HandyMenuID
                    ,menu.HandyMenuName
                    ,scanResult.SupplierKanbanID
                    ,scanResult.NumberOfInputBoxes
                    ,scanResult.FirstScanedString
                    ,scanResult.SecondScanedString
                    ,FORMAT(scanResult.ScanedAt, 'yyyy/MM/dd HH:mm:ss') AS ScanedAt
                    ,scanResult.CreatedAt AS ScanCreatedAt
                    ,scanResult.CreatedBy AS ScanCreatedBy
                    
                FROM D_Shipment shipment
                LEFT JOIN D_ShipmentSchedule schedule
	                ON shipment.ShipmentScheduleID = schedule.ShipmentScheduleID AND schedule.IsDeleted = 0
                INNER JOIN D_ScanResult scanResult
                    ON shipment.ScanResultID = scanResult.ScanResultID
                INNER JOIN M_HandyMenu menu 
					ON scanResult.HandyMenuID = menu.HandyMenuID
                INNER JOIN M_Company company
                    ON schedule.CompanyID = company.CompanyID 
                INNER JOIN M_Depo depo
                    ON schedule.DepoID = depo.DepoID
                WHERE 
	                shipment.ShipmentScheduleID = {shipmentScheduleId}
	                AND schedule.DepoID = {depoId}
	                AND schedule.CompanyID = {deliveryId}
                    AND menu.IsDeleted = 0
	                AND company.IsDeleted = 0
	                AND depo.IsDeleted = 0
                ORDER BY 
                    shipment.SupplierProductNumber ASC
            ";

            return sql;
        }
    }
}
