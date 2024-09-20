using Dapper;
using  ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace  ai_truck_load_measurement.Commons
{
    public static class StockStatusConnectController
    {
        /// <summary>
        /// 在庫照会情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<StockStatusModel> ConnectStockStatus(string sql, string databaseName)
        {
            // 戻り値
            List<StockStatusModel> strList = new();

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

                    strList = connection.Query<StockStatusModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番別ロット番号の数量チェック
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <param name="databaseName">データベース名</param>
        public static bool CheckLotNumberRemainQuantity(string searchDate, int depoId, int supplierId, string supplierProductNumber, string databaseName)
        {
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    // 品番別ロット番号の数量チェック
                    var checkSql = CreateSQLToCheckLotNumberRemainQuantity(searchDate, depoId, supplierId, supplierProductNumber);
                    var count = connection.ExecuteScalar(checkSql);
                    return Convert.ToInt32(count) > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 在庫情報取得SQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetStockStatus(string searchDate, int depoId, int supplierId, string supplierProductNumber = "")
        {
            var supplierProductNumberCondition = string.Empty;
            if(!string.Empty.Equals(supplierProductNumber))
            {
                supplierProductNumberCondition = $@" AND product.SupplierProductNumber = '{supplierProductNumber}' ";
            }
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}';
                DECLARE @SearchStartDate DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0);
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @LastMonthDate DATETIME = DATEADD(DAY, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0));
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                WITH 
                SearchData AS 
                (
	                SELECT 
		                searchInfo.SupplierProductNumber
		                ,STRING_AGG(searchInfo.LotNumber, '') AS LotNumber
		                ,SUM(searchInfo.InNumberOfBoxes) AS  SearchStoreInNumberOfBoxes
		                ,SUM(searchInfo.InQuantity) AS  SearchStoreInQuantity
		                ,SUM(searchInfo.OutNUmberOfBoxes) AS SearchStoreOutNumberOfBoxes
		                ,SUM(searchInfo.OutQuantity) AS  SearchStoreOutQuantity
	                FROM
	                (
		                SELECT
			                searchByDateInfo.WorkDate
			                ,searchByDateInfo.SupplierProductNumber
			                ,SUM(searchByDateInfo.InNumberOfBoxes) AS  InNumberOfBoxes
			                ,SUM(searchByDateInfo.InQuantity) AS InQuantity
			                ,SUM(searchByDateInfo.OutNumberOfBoxes) AS OutNumberOfBoxes
			                ,SUM(searchByDateInfo.OutQuantity) AS OutQuantity
			                ,CASE WHEN (SUM(searchByDateInfo.InNumberOfBoxes) - SUM(searchByDateInfo.OutNumberOfBoxes)) > 0 
                                THEN searchByDateInfo.LotNumber
				                ELSE ''
                            END AS LotNumber
		                FROM 
		                (
			                SELECT
				                storeIn.StoreInDate AS WorkDate
				                ,storeIn.SupplierProductNumber
				                ,COALESCE(SUM(storeIn.NumberOfBoxes), 0) AS InNumberOfBoxes
				                ,COALESCE(SUM(storeIn.Quantity), 0) AS InQuantity
				                ,0 AS OutNUmberOfBoxes
				                ,0 AS OutQuantity
				                ,storeIn.LotNumber
			                FROM D_StoreIn AS storeIn 
			                WHERE 
				                storeIn.StoreInDate >= @SearchStartDate AND 
                                storeIn.StoreInDate <= @SearchEndDate 
				                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
			                GROUP BY storeIn.SupplierProductNumber, storeIn.StoreInDate, storeIn.LotNumber
			                UNION ALL
			                SELECT 
				                storeOut.StoreOutDate AS WorkDate
				                ,storeOut.SupplierProductNumber
				                ,0 AS InNumberOfBoxes
				                ,0 AS InQuantity
				                ,COALESCE(SUM(storeOut.NumberOfBoxes), 0) AS OutNUmberOfBoxes
				                ,COALESCE(SUM(storeOut.Quantity), 0) AS OutQuantity
				                ,storeOut.LotNumber
			                FROM D_StoreOut AS storeOut
			                WHERE 
				                storeOut.StoreOutDate >= @SearchStartDate AND 
                                storeOut.StoreOutDate <= @SearchEndDate
				                AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
			                GROUP BY storeOut.SupplierProductNumber, storeOut.StoreOutDate, storeOut.LotNumber
		                ) AS searchByDateInfo
		                GROUP BY searchByDateInfo.SupplierProductNumber, searchByDateInfo.WorkDate, searchByDateInfo.LotNumber
	                ) AS searchInfo
	                GROUP BY searchInfo.SupplierProductNumber
                ),

                TotalData AS 
                (
	                SELECT 
	                    Total.SupplierProductNumber 
	                    ,COALESCE(SUM(Total.InNumberOfBoxes), 0) AS TotalStoreInNumberOfBoxes
	                    ,COALESCE(SUM(Total.InQuantity), 0) AS TotalStoreInQuantity
	                    ,COALESCE(SUM(Total.OutNUmberOfBoxes), 0) AS TotalStoreOutNumberOfBoxes
	                    ,COALESCE(SUM(Total.OutQuantity), 0) AS TotalStoreOutQuantity
	                FROM 
	                (
	                    SELECT storeIn.SupplierProductNumber, SUM(storeIn.NumberOfBoxes) AS InNumberOfBoxes, SUM(storeIn.Quantity) AS InQuantity, 0 AS OutNUmberOfBoxes, 0 AS OutQuantity
	                    FROM D_StoreIn AS storeIn 
	                    WHERE 
		                    storeIn.StoreInDate <= @LastMonthDate
		                    AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
	                    GROUP BY storeIn.SupplierProductNumber
	 
	                    UNION ALL
	                    SELECT storeOut.SupplierProductNumber , 0 AS InNumberOfBoxes, 0 AS InQuantity, SUM(storeOut.NumberOfBoxes) AS OutNUmberOfBoxes, SUM(storeOut.Quantity) AS OutQuantity
	                    FROM D_StoreOut AS storeOut
	                    WHERE
		                    storeOut.StoreOutDate <= @LastMonthDate
		                    AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
	                    GROUP BY storeOut.SupplierProductNumber
	                ) AS Total
	                GROUP BY Total.SupplierProductNumber
                ) 
                SELECT
                    product.ProductID -- 品番ID
                    ,@DepoId as DepoID -- 倉庫ID
                    ,product.SupplierID -- 仕入先ID
                    ,company.CompanyName AS SupplierName -- 仕入先名
                    ,product.SupplierProductNumber  -- 仕入先品番
                    ,product.LotQuantity -- 収容数
                    ,search.LotNumber --ロット番号
                    ,COALESCE(total.TotalStoreInQuantity - total.TotalStoreOutQuantity, 0) AS StockQuantityAtBeginningMonth -- 月初在庫数
                    ,COALESCE(search.SearchStoreInNumberOfBoxes, 0) AS StoreInNumberOfBoxes -- 入庫箱数
                    ,COALESCE(search.SearchStoreOutNumberOfBoxes, 0) AS StoreOutNumberOfBoxes -- 出庫箱数
                    ,COALESCE(search.SearchStoreInQuantity, 0) AS StoreInQuantity -- 入庫数
                    ,COALESCE(search.SearchStoreOutQuantity, 0) AS StoreOutQuantity -- 出庫数
                FROM M_Product product
                LEFT JOIN TotalData total on product.SupplierProductNumber = total.SupplierProductNumber
                LEFT JOIN SearchData AS search on product.SupplierProductNumber = search.SupplierProductNumber
                INNER JOIN M_Company AS company on product.SupplierID = company.CompanyID
                INNER JOIN R_DepoProduct AS depoProduct ON product.ProductID = depoProduct.ProductID AND depoProduct.DepoID = @DepoId
                WHERE
	                product.IsDeleted = 0
                    AND product.SupplierID = @CompanyId
                    {supplierProductNumberCondition}
                Order by 
	                product.SupplierProductNumber ASC
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先品番が一致する在庫情報取得SQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetStockStatusByProductNumber(string searchDate, int depoId, int supplierId, string supplierProductNumber)
        {
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}'; 
                DECLARE @SearchStartDate DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0);
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @LastMonthDate DATETIME = DATEADD(DAY, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0));
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                DECLARE @ProductNumber nvarchar(50) = '{supplierProductNumber}';
                SELECT
	                searchInfo.SupplierProductNumber
	                ,WorkedDate
	                ,COALESCE(SUM(searchInfo.InNumberOfBoxes), 0)  AS StoreInNumberOfBoxes -- 入庫箱数
	                ,COALESCE(SUM(searchInfo.InQuantity), 0) AS StoreInQuantity -- 入庫数量
	                ,COALESCE(SUM(searchInfo.OutNUmberOfBoxes), 0) AS StoreOutNumberOfBoxes -- 出庫箱数
	                ,COALESCE(SUM(searchInfo.OutQuantity), 0) AS StoreOutQuantity -- 出庫数量
                FROM 
                (
		                SELECT
		                storeIn.StoreInDate AS WorkedDate, storeIn.SupplierProductNumber 
		                ,SUM(storeIn.NumberOfBoxes) AS InNumberOfBoxes, 0 AS OutNUmberOfBoxes
		                ,SUM(storeIn.Quantity) AS InQuantity, 0 AS OutQuantity
	                FROM D_StoreIn AS storeIn 
	                WHERE 
		                storeIn.StoreInDate >= @SearchStartDate AND storeIn.StoreInDate <= @SearchEndDate 
		                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
		                AND storeIn.SupplierProductNumber = @ProductNumber
	                GROUP BY storeIn.SupplierProductNumber, storeIn.StoreInDate
	                UNION ALL
	                SELECT 
		                storeOut.StoreOutDate AS WorkedDate ,storeOut.SupplierProductNumber, 
		                NULL AS InNumberOfBoxes, SUM(storeOut.NumberOfBoxes) AS OutNUmberOfBoxes
		                ,0 AS InQuantity, SUM(storeOut.Quantity) AS OutQuantity
	                FROM D_StoreOut AS storeOut
	                WHERE 
		                storeOut.StoreOutDate >= @SearchStartDate AND storeOut.StoreOutDate <= @SearchEndDate
		                AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
		                AND storeOut.SupplierProductNumber = @ProductNumber
	                GROUP BY storeOut.SupplierProductNumber, storeOut.StoreOutDate
                ) AS searchInfo
                GROUP BY searchInfo.SupplierProductNumber, WorkedDate
                ORDER BY WorkedDate ASC
            ";
            return sql;
        }

        /// <summary>
        /// 品番別ロット番号一覧取得SQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetLotNumberDetailByProductNumber(string searchDate, int depoId, int supplierId, string supplierProductNumber)
        {
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}'; 
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @LastMonthDate DATETIME = DATEADD(DAY, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0));
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                DECLARE @ProductNumber nvarchar(50) = '{supplierProductNumber}';
                WITH 
                storeIn AS 
                (
	                SELECT
		                storeIn.LotNumber AS LotNumber
		                --,storeIn.StoreInDate AS WorkedDate, 
		                ,storeIn.SupplierProductNumber 
		                ,COALESCE(SUM(storeIn.NumberOfBoxes), 0) AS InNumberOfBoxes, 0 AS OutNUmberOfBoxes
		                ,COALESCE(SUM(storeIn.Quantity), 0) AS InQuantity , 0 AS OutQuantity
	                FROM D_StoreIn AS storeIn 
	                WHERE
                        storeIn.StoreInDate <= @SearchEndDate 
		                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
		                AND storeIn.SupplierProductNumber = @ProductNumber
		                AND storeIn.LotNumber is not null
		                AND storeIn.LotNumber <> ''
	                GROUP BY LotNumber, storeIn.SupplierProductNumber
                ),
                storeOut AS 
                (
	                SELECT
		                storeOut.LotNumber AS LotNumber,
		                --,storeOut.StoreOutDate AS WorkedDate, 
		                storeOut.SupplierProductNumber, 
		                0 AS InNumberOfBoxes, COALESCE(SUM(storeOut.NumberOfBoxes), 0) AS OutNUmberOfBoxes,
		                0 AS InQuantity, COALESCE(SUM(storeOut.Quantity), 0) AS OutQuantity
	                FROM D_StoreOut AS storeOut
	                WHERE
                        storeOut.StoreOutDate <= @SearchEndDate
		                AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
		                AND storeOut.SupplierProductNumber = @ProductNumber
		                AND storeOut.LotNumber is not null
		                AND storeOut.LotNumber <> ''
	                GROUP BY LotNumber, storeOut.SupplierProductNumber
                )

                SELECT 
                    storeIn.LotNumber
                    ,COALESCE(SUM(storeIn.InNumberOfBoxes), 0) AS StoreInNumberOfBoxes -- 入庫箱数
                    ,COALESCE(SUM(storeOut.OutNUmberOfBoxes), 0) AS StoreOutNumberOfBoxes -- 出庫箱数
                    ,COALESCE(SUM(storeIn.InQuantity), 0) AS StoreInQuantity -- 入庫数量
                    ,COALESCE(SUM(storeOut.OutQuantity), 0) AS StoreOutQuantity -- 出庫箱数
                    ,(COALESCE(SUM(storeIn.InNumberOfBoxes), 0) - COALESCE(SUM(storeOut.OutNUmberOfBoxes), 0)) AS StockRemainNumberOfBoxes
                    ,(COALESCE(SUM(storeIn.InQuantity), 0) - COALESCE(SUM(storeOut.OutQuantity), 0)) AS StockRemainQuantity
                FROM storeIn
                LEFT JOIN storeOut 
	                ON storeIn.SupplierProductNumber = storeOut.SupplierProductNumber 
	                and storeIn.LotNumber = storeOut.LotNumber
                GROUP BY storeIn.LotNumber
            ";
            return sql;
        }

        /// <summary>
        /// 品番別ロット番号の数量チェックSQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToCheckLotNumberRemainQuantity(string searchDate, int depoId, int supplierId, string supplierProductNumber)
        {
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}';
                DECLARE @SearchStartDate DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0);
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                DECLARE @ProductNumber nvarchar(50) = '{supplierProductNumber}';
                SELECT 
	                COUNT(*) as count 
                FROM 
                (
	                SELECT
		                LotNumber, (SUM(InQuantity) - SUM(OutQuantity)) AS RemainQuantity 
	                FROM 
	                (
		                SELECT 
		                    storeIn.LotNumber, sum(storeIn.Quantity) as InQuantity, 0 as OutQuantity
		                FROM D_StoreIn AS storeIn
		                WHERE
			                SupplierProductNumber = @ProductNumber
			                AND storeIn.StoreInDate >= @SearchStartDate AND storeIn.StoreInDate <= @SearchEndDate 
			                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
		                GROUP BY storeIn.LotNumber
		                UNION ALL
		                SELECT 
		                    storeOut.LotNumber, 0 as InQuantity, sum(storeOut.Quantity) as OutQuantity
		                FROM D_StoreOut AS storeOut
		                WHERE 
		                    SupplierProductNumber = @ProductNumber
		                    AND storeOut.StoreOutDate >= @SearchStartDate AND storeOut.StoreOutDate <= @SearchEndDate
		                    AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
		                GROUP BY storeOut.LotNumber
	                ) AS store_sum
	                GROUP BY LotNumber
                ) AS CheckData
                WHERE
	                LotNumber <> '' AND LotNumber IS NOT NULL
                    AND RemainQuantity > 0
            ";
            return sql;
        }
    }
}
