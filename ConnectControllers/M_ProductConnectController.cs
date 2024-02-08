using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    public class M_ProductConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>商品情報</returns>
        public static List<M_ProductModel> ConnectMProducts(string sql, string databaseName)
        {
            // 戻り値のリスト
            List<M_ProductModel> productList = new List<M_ProductModel>();

            try
            {
                // SQL Server接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);

                // SQL Serverに接続
                using (var connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // SQLを実行し、結果を取得
                    productList = connection.Query<M_ProductModel>(sql).ToList();
                }

                // 商品情報のリストを返す
                return productList;
            }
            catch (Exception)
            {
                // 例外を処理する
                throw;
            }
        }

        /// <summary>
        /// 品番マスターの詳細を取得する
        /// </summary>
        /// <param name="productList">品番情報</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>品番情報</returns>
        public static List<M_ProductModel> GetRDepoProducts(List<M_ProductModel> productList, string databaseName)
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

                    if (productList.Count > 0)
                    {
                        foreach (M_ProductModel item in productList)
                        {
                            // 倉庫-品番中間取得
                            var depoProductSql = CreateSQLToGetRDepoProducts(item.ProductID);
                            List<M_DepoModel> depoList = connection.Query<M_DepoModel>(depoProductSql).ToList();
                            if (depoList.Count > 0)
                            {
                                item.RDepoProducts = depoList;
                            }
                        }
                    }
                }
                return productList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番マスター削除
        /// </summary>
        /// <param name="companyId">品番ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>更新件数</returns>
        public static int DeleteMProduct(int companyId, string databaseName)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                // DB接続
                try
                {
                    // 品番マスター削除SQL作成
                    string deleteSql = CreateSQLToDeleteMCompany(companyId);
                    // 品番マスター削除
                    int deleteAffectedRows = connection.Execute(deleteSql);
                    // 更新件数が0の場合はエラーとする
                    if (deleteAffectedRows == 0)
                    {
                        // エラーコード：E2011
                        throw new Exception();
                    }

                    return deleteAffectedRows;
                }
                catch (Exception ex)
                {
                    // エラーコード：E2011
                    throw ex;
                }
            }
        }

        // <summary>
        /// 品番マスター削除SQL作成
        /// </summary>
        /// <param name="companyId">品番ID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMCompany(int productId)
        {
            var sql = $@"
                UPDATE M_Product
                SET IsDeleted = 1
                WHERE ProductID = {productId}
            ;";
            return sql;
        }

        /// <summary>
        /// 倉庫-品番中間テーブル情報取得SQL作成
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetRDepoProducts(int productId)
        {
            var sql = $@"
                SELECT 
	                depoProduct.DepoID,
	                mDepo.DepoCode,
	                mDepo.DepoName
                FROM 
	                R_DepoProduct AS depoProduct
                INNER JOIN M_Depo AS mDepo 
                    ON depoProduct.DepoID = mDepo.DepoID
                WHERE 
	                depoProduct.ProductID = {productId}
                    AND mDepo.IsDeleted = 0
            ";
            return sql;
        }


        /// <summary>
        /// 品番SELECT文SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMProducts()
        {
            var sql = $@"
                SELECT
                    product.ProductID,
                    product.SupplierID,
	                supplier.CompanyName AS SupplierName,
                    product.SupplierProductNumber,
                    product.DeliveryID,
	                delivery.CompanyName AS DeliveryName,
                    product.DeliveryProductNumber,
                    product.ProductName,
                    product.LotQuantity,
                    product.AllowedDuplicatesFlag,
                    product.IsDeleted,
                    product.CreatedAt,
                    product.CreatedBy,
                    product.UpdatedAt,
                    product.UpdatedBy
                FROM
                    M_Product AS product
                INNER JOIN M_Company AS supplier ON product.SupplierID = supplier.CompanyID
                INNER JOIN M_Company AS delivery ON product.DeliveryID = delivery.CompanyID
                WHERE
                    product.IsDeleted = 0
                    AND supplier.IsDeleted = 0
	                AND delivery.IsDeleted = 0
                ORDER BY
                    product.ProductID ASC;
            ;";

            return sql;
        }
    }
}
