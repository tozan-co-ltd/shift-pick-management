using Dapper;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
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
        /// 在庫情報取得SQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetStockStatus(string searchDate, int depoId, int supplierId, string productNumber = "")
        {
            var productNumberCondition = string.Empty;
            if(!string.Empty.Equals(productNumber))
            {
                productNumberCondition = $@" AND product.SupplierProductNumber = '{productNumber}' ";
            }
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}'; 
                DECLARE @SearchStartDate DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0);
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @LastMonthDate DATETIME = DATEADD(DAY, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0));
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                --SELECT @SearchStartDate, @SearchEndDate, @LastMonthDate;
                WITH 
                SearchData AS 
                (
	                SELECT
		                searchInfo.SupplierProductNumber 
		                ,SUM(searchInfo.InNumberOfBoxes) AS  SearchStoreInNumberOfBoxes
		                ,SUM(searchInfo.OutNUmberOfBoxes) AS SearchStoreOutNumberOfBoxes
	                FROM 
	                (
	                 SELECT storeIn.SupplierProductNumber ,SUM(storeIn.NumberOfBoxes) AS InNumberOfBoxes , NULL AS OutNUmberOfBoxes
	                 FROM D_StoreIn AS storeIn 
	                 WHERE 
		                storeIn.StoreInDate >= @SearchStartDate AND storeIn.StoreInDate <= @SearchEndDate 
		                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
	                 GROUP BY storeIn.SupplierProductNumber
	                 UNION ALL
	                 SELECT storeOut.SupplierProductNumber, NULL AS InNumberOfBoxes, SUM(storeOut.NumberOfBoxes) AS OutNUmberOfBoxes
	                 FROM D_StoreOut AS storeOut
	                 WHERE 
		                storeOut.StoreOutDate >= @SearchStartDate AND storeOut.StoreOutDate <= @SearchEndDate
		                AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
	                 GROUP BY storeOut.SupplierProductNumber
	                ) AS searchInfo
	                GROUP BY searchInfo.SupplierProductNumber
                ),

                TotalData AS 
                (
	                SELECT 
	                Total.SupplierProductNumber 
	                ,COALESCE(SUM(Total.InNumberOfBoxes), 0) AS TotalStoreInNumberOfBoxes
	                ,COALESCE(SUM(Total.OutNUmberOfBoxes), 0) AS TotalStoreOutNumberOfBoxes
	                FROM 
	                (
	                 SELECT storeIn.SupplierProductNumber, SUM(storeIn.NumberOfBoxes) AS InNumberOfBoxes, NULL AS OutNUmberOfBoxes
	                 FROM D_StoreIn AS storeIn 
	                 WHERE 
		                storeIn.StoreInDate <= @LastMonthDate
		                AND storeIn.CompanyID = @CompanyId AND storeIn.DepoID = @DepoId AND storeIn.IsDeleted = 0
	                 GROUP BY storeIn.SupplierProductNumber
	 
	                 UNION ALL
	                 SELECT storeOut.SupplierProductNumber , NULL AS InNumberOfBoxes , SUM(storeOut.NumberOfBoxes) AS OutNUmberOfBoxes
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
                ,COALESCE((total.TotalStoreInNumberOfBoxes - total.TotalStoreOutNumberOfBoxes) * product.LotQuantity, 0) AS StockQuantityAtBeginningMonth -- 月初在庫数
                ,COALESCE(search.SearchStoreInNumberOfBoxes, 0) AS StoreInNumberOfBoxes -- 入庫箱数
                ,COALESCE(search.SearchStoreOutNumberOfBoxes, 0) AS StoreOutNumberOfBoxes -- 出庫箱数
                ,COALESCE(search.SearchStoreInNumberOfBoxes * product.LotQuantity, 0) AS StoreInQuantity -- 入庫数
                ,COALESCE(search.SearchStoreOutNumberOfBoxes* product.LotQuantity, 0) AS StoreOutQuantity -- 出庫数
                FROM M_Product product
                LEFT JOIN TotalData total on product.SupplierProductNumber = total.SupplierProductNumber
                LEFT JOIN SearchData AS search on product.SupplierProductNumber = search.SupplierProductNumber
                INNER JOIN M_Company AS company on product.SupplierID = company.CompanyID
                WHERE
	                product.IsDeleted = 0
                    AND product.SupplierID = @CompanyId
                    {productNumberCondition}
                Order by 
	                product.SupplierProductNumber ASC
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先品番で在庫情報取得SQL作成
        /// </summary>
        /// <param name="searchDate">年月日</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <param name="productNumber">仕入先品番</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetStockStatusByProductNumber(string searchDate, int depoId, int supplierId, string productNumber)
        {
            var sql = $@"
                DECLARE @InputDate DATE = '{searchDate}'; 
                DECLARE @SearchStartDate DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0);
                DECLARE @SearchEndDate DATETIME = CONVERT(DATETIME, CONVERT(VARCHAR(10), @InputDate) + ' 23:59:59');
                DECLARE @LastMonthDate DATETIME = DATEADD(DAY, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InputDate), 0));
                DECLARE @CompanyId int = {supplierId};
                DECLARE @DepoId int = {depoId};
                DECLARE @ProductNumber nvarchar(50) = '{productNumber}';
                SELECT
		            searchInfo.SupplierProductNumber
		            ,WorkedDate
		            ,COALESCE(SUM(searchInfo.InNumberOfBoxes), 0)  AS StoreInNumberOfBoxes -- 入庫箱数
		            ,COALESCE(SUM(searchInfo.OutNUmberOfBoxes), 0) AS StoreOutNumberOfBoxes -- 出庫箱数
	            FROM 
	            (
			            SELECT 
			            storeIn.StoreInDate AS WorkedDate ,storeIn.SupplierProductNumber 
			            ,SUM(storeIn.NumberOfBoxes) AS InNumberOfBoxes , NULL AS OutNUmberOfBoxes
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
		            FROM D_StoreOut AS storeOut
		            WHERE 
			            storeOut.StoreOutDate >= @SearchStartDate AND storeOut.StoreOutDate <= @SearchEndDate
			            AND storeOut.CompanyID = @CompanyId AND storeOut.DepoID = @DepoId AND storeOut.IsDeleted = 0
			            AND storeOut.SupplierProductNumber = @ProductNumber
		            GROUP BY storeOut.SupplierProductNumber, storeOut.StoreOutDate
	            ) AS searchInfo
	            GROUP BY searchInfo.SupplierProductNumber, WorkedDate
                Order BY WorkedDate ASC
            ";
            return sql;
        }
    }
}
