using Dapper;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

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
                --先に出庫実績を集計
                WITH storeout_sum AS (
                    SELECT 
                        DepoID,
                        DeliveryDate,
                        DeliveryTimeClass,
                        DeliverySlipNumber,
                        DeliveryProductNumber,
                        COALESCE(SUM(NumberOfBoxes), 0) AS StoreOutNumberOfBoxes, -- 出庫箱数
                        COALESCE(SUM(Quantity), 0) AS StoreOutQuantity -- 出庫数量
                    FROM 
                        D_StoreOut
                    WHERE 
                        DepoID = {depoId}
                        AND DeliveryDate = '{nextDay}'
                        AND IsDeleted = 0
                    GROUP BY 
                        DepoID,
                        DeliveryDate,
                        DeliveryTimeClass,
                        DeliverySlipNumber,
                        DeliveryProductNumber
                )
                
                --出荷指示と出庫実績を紐づけ
                SELECT  
                    shipment.ShipmentScheduleID
                   ,depo.DepoName
                   ,company.CompanyName AS SupplierName
                   ,COALESCE(storeout_sum.StoreOutNumberOfBoxes,0) as StoreOutNumberOfBoxes -- 出庫箱数
                   ,COALESCE(storeout_sum.StoreOutQuantity,0) as StoreOutQuantity -- 出庫数量    
                   ,ROUND(shipment.Quantity / shipment.LotQuantity, 0) AS ScheduleNumberOfBoxes -- 指示箱数
                   ,shipment.Quantity AS ScheduleQuantity -- 納入指示数
                   ,shipment.*
                FROM 
                    D_ShipmentSchedule AS shipment
                LEFT JOIN 
                    storeout_sum  ON shipment.DepoID = storeout_sum.DepoID
                    AND shipment.DeliveryDate = storeout_sum.DeliveryDate
                    AND shipment.DeliveryTimeClass = storeout_sum.DeliveryTimeClass
                    AND shipment.DeliverySlipNumber = storeout_sum.DeliverySlipNumber
                    AND shipment.DeliveryProductNumber = storeout_sum.DeliveryProductNumber                    
                INNER JOIN 
                    M_Company AS company ON shipment.CompanyID = company.CompanyID
                INNER JOIN 
                    M_Depo AS depo ON shipment.DepoID = depo.DepoID
                WHERE
                 shipment.DepoID =  {depoId}
                 AND shipment.CompanyID ={companyId}
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

                    bool insertFlg = true;
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
            if (model.DifferenceCountCheck)
            {
                differenceCheckCondition = " AND shipment_schedule.NumberOfBoxes <> COALESCE(storeout_sum.StoreOutNumberOfBoxes, 0) ";
            }

            model.SearchEndDate = string.Concat(model.SearchEndDate, " 23:59:59");
            var sql = $@"          
                -- 出荷計画を絞り込み
					WITH 
					shipment_schedule as
					(
					    SELECT * FROM 
					    D_ShipmentSchedule
					    WHERE 
					      DepoID = {model.SelectedDepoID}
                        AND CompanyID = {model.SelectedCompanyID}
                        AND DeliveryDate >= '{model.SearchStartDate}'
                        AND DeliveryDate <= '{model.SearchEndDate}'
	                    AND IsDeleted = 0
					),
					-- 出庫実績を絞り込んで集計
					storeout_sum as 
					(
						SELECT  DepoID,DeliveryDate,DeliverySlipNumber,DeliveryProductNumber,
						    SUM(COALESCE(NumberOfBoxes, 0)) AS StoreOutNumberOfBoxes -- 出庫箱数
	                        ,SUM(COALESCE(Quantity, 0)) AS StoreOutQuantity --出庫数量
						FROM D_StoreOut 
						WHERE IsDeleted = 0
						AND DepoID ={model.SelectedDepoID}				
						AND DeliveryDate >='{model.SearchStartDate}'
						AND DeliveryDate <= '{model.SearchEndDate}'
					GROUP BY  
					    DepoID,DeliveryDate,DeliverySlipNumber,DeliveryProductNumber				
					)


					--両テーブルを外部結合
					SELECT
	               shipment_schedule.ShipmentScheduleID
	                ,shipment_schedule.CompanyID AS CompanyID
	                ,company.CompanyName AS DeliveryName
                    ,depo.DepoID
	                ,depo.DepoName
					,shipment_schedule.DeliveryDate
					,shipment_schedule.DeliveryTimeClass
					,shipment_schedule.DeliveryProductNumber
					,shipment_schedule.SupplierProductNumber
					,shipment_schedule.LotQuantity
					,shipment_schedule.OrdererCode
					,shipment_schedule.OrdererFactoryKubun
					,shipment_schedule.OrdererName
					,shipment_schedule.OrdererFactoryName
					,shipment_schedule.ShipperCode
					,shipment_schedule.ShipperFactoryKubun
					,shipment_schedule.ShipperName
					,shipment_schedule.DeliveryCode
					,shipment_schedule.DeliveryFactoryKubun
					,shipment_schedule.DeliveryLocation
					,shipment_schedule.DeliveryName AS NameOfDelivery
					,shipment_schedule.DeliveryFactoryName
					,shipment_schedule.RegularKubun
					,shipment_schedule.IssuedDate
					,shipment_schedule.DeliveryTime
					,shipment_schedule.TranspotationIdentify
					,shipment_schedule.DeliverySlipNumber
					,shipment_schedule.DeliverySlipPageNumber
					,shipment_schedule.DeliverySlipRowNumber
					,shipment_schedule.DeliveryProductAbbreviation
					,shipment_schedule.DeliveryProductName
					,shipment_schedule.BranchNumber
					,shipment_schedule.UpdatedAt
					,shipment_schedule.UpdatedBy
					,COALESCE(shipment_schedule.NumberOfBoxes, 0) AS NumberOfBoxes
					,COALESCE(shipment_schedule.Quantity, 0) AS Quantity
	                ,COALESCE(StoreOutNumberOfBoxes, 0) as StoreOutNumberOfBoxes -- 出庫箱数合計
	                ,COALESCE(StoreOutQuantity, 0) as StoreOutQuantity --出庫数量合計
                FROM shipment_schedule
                LEFT OUTER  JOIN 
				 storeout_sum 
		                ON shipment_schedule.DepoID = storeout_sum.DepoID
		                AND	shipment_schedule.DeliveryDate = storeout_sum.DeliveryDate
                        AND shipment_schedule.DeliverySlipNumber = storeout_sum.DeliverySlipNumber
                        AND shipment_schedule.DeliveryProductNumber = storeout_sum.DeliveryProductNumber                        
                INNER JOIN M_Company AS company 
                        ON shipment_schedule.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo 
                    ON shipment_schedule.DepoID = depo.DepoID
                WHERE                   
					company.IsDeleted = 0
                    AND depo.IsDeleted = 0					
                    {differenceCheckCondition}         
					ORDER BY 
	                shipment_schedule.DeliveryProductNumber ASC 
            ";
            return sql;
        }

        /// <summary>
        /// 出荷指示削除SQL作成
        /// </summary>
        /// <param name="shipmentScheduleId">出荷指示ID</param>
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
