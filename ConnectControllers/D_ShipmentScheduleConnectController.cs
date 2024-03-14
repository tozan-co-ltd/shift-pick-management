using Dapper;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Reflection;
using System.Transactions;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 出荷指示テーブルに関する関数
    /// </summary>
    public static class D_ShipmentScheduleConnectController
    {
        /// <summary>
        /// 出荷指示情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_ShipmentScheduleModel> ConnectDShipmentSchedules(string sql, string databaseName)
        {
            // 戻り値
            List<D_ShipmentScheduleModel> strList = new();

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

                    strList = connection.Query<D_ShipmentScheduleModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 出荷指示に対する作業進捗情報
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDShipmentSchedulesForWorkProgressInformation(int depoId, int companyId, string nextDay)
        {
            var sql = $@"
                SELECT  
                    depo.DepoName
	                ,company.CompanyName AS SupplierName
	                ,COALESCE(storeOut.NumberOfBoxes, 0) AS StoreOutNumberOfBoxes --出庫箱数
	                ,COALESCE(storeOut.Quantity, 0) AS StoreOutQuantity --出庫数量
					,ROUND(shipment.Quantity / shipment.LotQuantity, 0, 0) AS ScheduleNumberOfBoxes --指示箱数
					,shipment.Quantity AS ScheduleQuantity --納入指示数
                    ,shipment.*
                FROM D_ShipmentSchedule AS shipment
                LEFT JOIN D_StoreOut AS storeOut
                    ON shipment.DepoID = storeOut.DepoID
                    AND shipment.DeliveryDate = storeOut.DeliveryDate
                    AND shipment.DeliveryTimeClass = storeOut.DeliveryTimeClass
                    AND shipment.DeliverySlipNumber = storeOut.DeliverySlipNumber
                    AND shipment.DeliveryProductNumber = storeOut.DeliveryProductNumber
                    AND storeOut.IsDeleted = 0
                INNER JOIN M_Company AS company
                    ON shipment.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo
                    ON shipment.DepoID = depo.DepoID
                WHERE
                    shipment.DepoID = {depoId}
                    AND shipment.CompanyID = {companyId}
                    AND shipment.DeliveryDate = '{nextDay}'
                    AND shipment.IsDeleted = 0
                    AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 出荷指示データ登録
        /// </summary>
        /// <param name="modelList">モデルリスト</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="importFileName">取込ファイル名</param>
        /// <param name="viewTitle">画面名</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns></returns>
        public static bool InsertDShipmentSchedule(List<D_ShipmentScheduleModel> modelList, int depoId, int companyId, string importFileName, string viewTitle, LoginUserModel loginUser)
        {
            bool insertFlg = false;

            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                SqlTransaction transaction = null;
                transaction = connection.BeginTransaction();

                // DB接続
                try
                {
                    DateTime systemDate = DateTime.Now;

                    foreach (var model in modelList)
                    {
                        // 出荷指示取込テーブル登録
                        string insertSql = CreateSQLToInsertDShipmentSchedule(model, depoId, companyId, systemDate, loginUser.UserName);
                        int affectRows = connection.Execute(insertSql, null, transaction);
                        // 更新件数が0の場合はエラーとする
                        if (affectRows == 0)
                        {
                            throw new Exception();
                        }
                    }

                    // ファイル取込実績テーブル登録
                    D_FileImportModel dFileImportModel = new()
                    {
                        DepoID = depoId,
                        MenuName = viewTitle,
                        ImportFileName = importFileName,
                        CreatedAt = systemDate,
                        CreatedBy = loginUser.UserName
                    };
                    string dFileImportInserSql = D_FileImportConnectController.CreateSQLToInsertDFileImport(dFileImportModel, systemDate, loginUser.UserName);
                    var insertAffectRows = connection.Execute(dFileImportInserSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertAffectRows == 0)
                    {
                        throw new Exception();
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    insertFlg = true;
                    return insertFlg;
                }
                catch (SqlException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 出荷指示削除
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>更新件数</returns>
        public static int DeleteDShipmentSchedule(int shipmentScheduleId, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                // DB接続
                try
                {
                    // 出荷指示削除
                    string deleteSql = CreateSQLToDeleteDShipmentSchedule(shipmentScheduleId, DateTime.Now, loginUser.UserName);
                    int affectedRows = connection.Execute(deleteSql);
                    // 更新件数が0の場合はエラーとする
                    if (affectedRows == 0)
                    {
                        throw new Exception();
                    }

                    return affectedRows;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 出荷指示データINSERT文SQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="userName">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDShipmentSchedule(D_ShipmentScheduleModel model, int depoId, int companyId, DateTime createdAt, string userName)
        {
            var sql = $@"

            BEGIN 
	            IF EXISTS (
		            SELECT 1
		            FROM D_ShipmentSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {companyId}
                        AND DeliveryDate = '{model.DeliveryDate}'
                        AND DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        AND IsDeleted = 0
	            )
	            BEGIN
		            DELETE FROM D_ShipmentSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {companyId}
                        AND DeliveryDate = '{model.DeliveryDate}'
                        AND DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND IsDeleted = 0
	            END
            
                BEGIN
                    INSERT INTO D_ShipmentSchedule
                    (
                        DepoID,
                        CompanyID,
                        OrdererCode,
                        OrdererFactoryKubun,
                        OrdererName,
                        OrdererFactoryName,
                        ShipperCode,
                        ShipperFactoryKubun,
                        ShipperName,
                        DeliveryCode,
                        DeliveryFactoryKubun,
                        DeliveryLocation,
                        DeliveryName,
                        DeliveryFactoryName,
                        RegularKubun,
                        IssuedDate,
                        DeliveryDate,
                        DeliveryTime,
                        DeliveryTimeClass,
                        TranspotationIdentify,
                        DeliverySlipNumber,
                        DeliverySlipPageNumber,
                        DeliverySlipRowNumber,
                        DeliveryProductNumber,
                        DeliveryProductAbbreviation,
                        DeliveryProductName,
                        LotQuantity,
                        BranchNumber,
                        Quantity,
                        SupplierProductNumber,
                        NumberOfBoxes,
                        CreatedAt,
                        CreatedBy,
                        UpdatedAt,
                        UpdatedBy
                    )
                    VALUES 
                    (
                        {depoId},
                        {companyId},
                        '{model.OrdererCode}',
                        '{model.OrdererFactoryKubun}',
                        '{model.OrdererName}',
                        '{model.OrdererFactoryName}',
                        '{model.ShipperCode}',
                        '{model.ShipperFactoryKubun}',
                        '{model.ShipperName}',
                        '{model.DeliveryCode}',
                        '{model.DeliveryFactoryKubun}',
                        '{model.DeliveryLocation}',
                        '{model.NameOfDelivery}',
                        '{model.DeliveryFactoryName}',
                        '{model.RegularKubun}',
                        '{model.IssuedDate}',
                        '{model.DeliveryDate}',
                        '{model.DeliveryTime}',
                        {model.DeliveryTimeClass},
                        '{model.TranspotationIdentify}',
                        '{model.DeliverySlipNumber}',
                        {model.DeliverySlipPageNumber},
                        {model.DeliverySlipRowNumber},
                        '{model.DeliveryProductNumber}',
                        '{model.DeliveryProductAbbreviation}',
                        '{model.DeliveryProductName}',
                        {model.LotQuantity},
                        {model.BranchNumber},
                        {model.Quantity},
                        '{model.SupplierProductNumber}',
                        {model.NumberOfBoxes},
                        '{createdAt}',
                        '{userName}',
                        '{createdAt}',
                        '{userName}'
                    );
                END

            END
            ";

            return sql;
        }

        /// <summary>
        /// 出荷実績重複チェック文SQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToIsExistDShipment(D_ShipmentScheduleModel model, int depoId, int companyId)
        {
            var sql = $@"
		        Declare @ShipmentScheduleID int;
                SET @ShipmentScheduleID = (
                    SELECT 
                        TOP 1 schedule.ShipmentScheduleID
		            FROM D_ShipmentSchedule AS schedule
		            WHERE 
                        DepoID = {depoId} 
                        AND schedule.CompanyID = {companyId}
                        AND schedule.DeliveryDate = '{model.DeliveryDate}'
                        AND schedule.DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND schedule.DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND schedule.DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        AND schedule.IsDeleted = 0
                );
                SELECT count(*)
                FROM D_Shipment AS shipment
                WHERE 
                    shipment.ShipmentScheduleID = @ShipmentScheduleID
            ;";

            return sql;
        }

        /// <summary>
        /// 出荷指示IDで出荷実績重複チェック文SQL作成
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToCheckExistDShipmentByShipmentScheduleId(int shipmentScheduleId)
        {
            var sql = $@"
		        SELECT
                    count(schedule.ShipmentScheduleID) AS count
                FROM 
                    D_ShipmentSchedule AS schedule
                INNER JOIN D_Shipment AS shipment
                    ON schedule.ShipmentScheduleID = shipment.ShipmentScheduleID 
                WHERE
                    shipment.ShipmentScheduleID = {shipmentScheduleId}
            ;";

            return sql;
        }

        /// <summary>
        /// 出庫実績の存在チェック文SQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToCheckExistDStoreOutByShipmentSchedule(D_ShipmentScheduleModel model)
        {
            var sql = $@"
		            SELECT
                        count(StoreOutID) AS count
                    FROM D_StoreOut
		            WHERE 
                        DepoID = {model.DepoID} 
                        AND DeliveryDate = '{model.DeliveryDate}'
                        AND DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        AND IsDeleted = 0
            ;";

            return sql;
        }

        /// <summary>
        /// 出荷指示IDで出荷指示照会取得
        /// </summary>
        /// <param name="id">出荷指示ID</param>
        /// <param name="databaseName">データベース名</param>
        public static D_ShipmentScheduleModel? GetDShipmentScheduleByShipmentScheduleId(int id, string databaseName)
        {
            // 戻り値
            D_ShipmentScheduleModel? result = new();

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

                    var sql = $@"
                        SELECT * 
                        FROM D_ShipmentSchedule 
                        WHERE 
                            ShipmentScheduleID = {id}
                            AND IsDeleted = 0
                    ";

                    result = connection.Query<D_ShipmentScheduleModel>(sql).FirstOrDefault();
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 出荷指示情報取得SQL作成
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDShipmentSchedules(D_ShipmentScheduleSearchModel model)
        {
            // 実績数不一致のみ
            string differenceCheckCondition = string.Empty;
            if (model.DiffenceCountCheck)
            {
                differenceCheckCondition = " AND shipment.NumberOfBoxes <> COALESCE(storeOut.NumberOfBoxes, 0) ";
            }

            // 便
            string binCondition = string.Empty;
            if (model.BinListInt != null && model.BinListInt.Count > 0)
            {
                binCondition = $@" AND shipment.DeliveryTimeClass in ({string.Join(",", model.BinListInt)})";
            }
            model.SearchEndDate = string.Concat(model.SearchEndDate, " 23:59:59");
            var sql = $@"
                SELECT
	                shipment.ShipmentScheduleID
	                ,shipment.CompanyID AS CompanyID
	                ,company.CompanyName AS DeliveryName
                    ,depo.DepoID
	                ,depo.DepoName
					,shipment.DeliveryDate
					,shipment.DeliveryTimeClass
					,shipment.DeliveryProductNumber
					,shipment.SupplierProductNumber
					,shipment.LotQuantity
					,shipment.OrdererCode
					,shipment.OrdererFactoryKubun
					,shipment.OrdererName
					,shipment.OrdererFactoryName
					,shipment.ShipperCode
					,shipment.ShipperFactoryKubun
					,shipment.ShipperName
					,shipment.DeliveryCode
					,shipment.DeliveryFactoryKubun
					,shipment.DeliveryLocation
					,shipment.DeliveryName AS NameOfDelivery
					,shipment.DeliveryFactoryName
					,shipment.RegularKubun
					,shipment.IssuedDate
					,shipment.DeliveryTime
					,shipment.TranspotationIdentify
					,shipment.DeliverySlipNumber
					,shipment.DeliverySlipPageNumber
					,shipment.DeliverySlipRowNumber
					,shipment.DeliveryProductAbbreviation
					,shipment.DeliveryProductName
					,shipment.BranchNumber
					,shipment.UpdatedAt
					,shipment.UpdatedBy
					,COALESCE(shipment.NumberOfBoxes, 0) AS NumberOfBoxes
					,COALESCE(shipment.Quantity, 0) AS Quantity
	                ,SUM(COALESCE(storeOut.NumberOfBoxes, 0)) AS StoreOutNumberOfBoxes -- 出庫箱数
	                ,SUM(COALESCE(storeOut.Quantity, 0)) AS StoreOutQuantity --出庫数量
                FROM D_ShipmentSchedule shipment
                LEFT JOIN D_StoreOut AS storeOut 
		                ON shipment.DepoID = storeOut.DepoID
		                AND	shipment.DeliveryDate = storeOut.DeliveryDate
                        AND shipment.DeliverySlipNumber = storeOut.DeliverySlipNumber
                        AND shipment.DeliveryProductNumber = storeOut.DeliveryProductNumber
                        AND storeOut.IsDeleted = 0
                INNER JOIN M_Company AS company 
                        ON shipment.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo 
                    ON shipment.DepoID = depo.DepoID
                WHERE 
                    shipment.DepoID = {model.SelectedDepoID}
                    AND shipment.CompanyID = {model.SelectedCompanyID}
                    AND shipment.DeliveryDate >= '{model.SearchStartDate}'
                    AND shipment.DeliveryDate <= '{model.SearchEndDate}'
                    {binCondition}
                    {differenceCheckCondition}
	                AND shipment.IsDeleted = 0
	                AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
                GROUP BY
					shipment.ShipmentScheduleID
	                ,shipment.CompanyID
	                ,company.CompanyName
                    ,depo.DepoID
	                ,depo.DepoName
					,shipment.ShipmentScheduleID
					,shipment.DeliveryName
					,shipment.DeliveryDate
					,shipment.DeliveryTimeClass
					,shipment.DeliveryProductNumber
					,shipment.SupplierProductNumber
					,shipment.LotQuantity
					,shipment.OrdererCode
					,shipment.OrdererFactoryKubun
					,shipment.OrdererName
					,shipment.OrdererFactoryName
					,shipment.ShipperCode
					,shipment.ShipperFactoryKubun
					,shipment.ShipperName
					,shipment.DeliveryCode
					,shipment.DeliveryFactoryKubun
					,shipment.DeliveryLocation
					,shipment.DeliveryName
					,shipment.DeliveryFactoryName
					,shipment.RegularKubun
					,shipment.IssuedDate
					,shipment.DeliveryTime
					,shipment.TranspotationIdentify
					,shipment.DeliverySlipNumber
					,shipment.DeliverySlipPageNumber
					,shipment.DeliverySlipRowNumber
					,shipment.DeliveryProductAbbreviation
					,shipment.DeliveryProductName
					,shipment.BranchNumber
					,shipment.UpdatedAt
					,shipment.UpdatedBy
                    ,shipment.NumberOfBoxes
					,shipment.Quantity
                ORDER BY 
	                shipment.DeliveryProductNumber ASC 
            ";
            return sql;
        }

        /// <summary>
        /// 出荷指示削除SQL作成
        /// </summary>
        /// <param name="storeInId">出荷指示ID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteDShipmentSchedule(int shipmentScheduleId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                        UPDATE D_ShipmentSchedule
                        SET 
                            IsDeleted = 1
                            ,UpdatedAt = '{updatedAt}'
                            ,UpdatedBy = '{updatedBy}'
                        WHERE 
                            ShipmentScheduleID = {shipmentScheduleId}
            ;";
            return sql;
        }
    }
}
