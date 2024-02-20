using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

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
            // 戻り値
            List<M_ProductModel> productList = new List<M_ProductModel>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);

                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    productList = connection.Query<M_ProductModel>(sql).ToList();
                }

                return productList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番情報取得SQL作成
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
                    product.IsDeleted,
                    product.CreatedAt,
                    product.CreatedBy,
                    product.UpdatedAt,
                    product.UpdatedBy,
                    (
		                SELECT STRING_AGG(mdepo.DepoName, ' , ')
		                FROM R_DepoProduct AS depoProduct
		                INNER JOIN M_Depo AS mdepo ON depoProduct.DepoID = mdepo.DepoID
		                WHERE depoProduct.ProductID = product.ProductID
	                ) AS RDepoProductNames
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
        /// 品番マスターの詳細を取得
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
                            // 倉庫-品番中間テーブル情報取得
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
        /// 仕入先品番が品番マスターに存在するかチェック
        /// </summary>
        /// <param name="supplierProductNumber">仕入先品番</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static bool CheckMProductExist(string? supplierProductNumber, string databaseName)
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
                    // SQL作成
                    string sql = $@"
                        SELECT COUNT(*) 
                        FROM M_Product AS product
                        WHERE 
                            product.SupplierProductNumber = '{supplierProductNumber}'
                            AND product.IsDeleted = 0
                    ";

                    var productCount = connection.ExecuteScalar<int>(sql);
                    if (productCount == 0)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 納入先品番から仕入先品番を取得
        /// </summary>
        /// <param name="deliveryId">納入先品番ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static M_ProductModel? GeProductByDeliveryProductNumber(int deliveryId, string? deliveryProductNumber, string databaseName)
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
                    // SQL作成
                    string sql = $@"
                        SELECT TOP 1 *
                        FROM M_Product
                        WHERE 
                            DeliveryID = {deliveryId}
                            AND DeliveryProductNumber = '{deliveryProductNumber}'
                            AND IsDeleted = 0
                    ";

                    var product = connection.QueryFirstOrDefault<M_ProductModel>(sql);
                    return product;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 品番マスター削除
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>更新件数</returns>
        public static void DeleteMProduct(int productId, string databaseName)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
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
                    // 品番マスター削除SQL作成
                    string productDeleteSql = CreateSQLToDeleteMCompany(productId);
                    // 品番マスター削除
                    int productDeleteCount = connection.Execute(productDeleteSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (productDeleteCount == 0)
                    {
                        throw new Exception();
                    }

                    // 倉庫-品番中間テーブル削除SQL作成
                    string depoProductDeleteSql = CreateSQLToDeleteRDepoProduct(productId);
                    // 倉庫-品番中間テーブル削除
                    int depoProductDelCount = connection.Execute(depoProductDeleteSql, null, transaction);
                    
                    // トランザクションのコミット
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 会社区分ごとに会社リストを取得
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetCompanysByCompanyKubun(int companyKubun, string databaseName)
        {
            var companyList = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT  
	                        CONCAT(CompanyName, ' - ', ClientName) AS Text
	                        ,CompanyID as Value
                        FROM M_Company
                        WHERE (1=1)
                            AND CompanyKubun = {companyKubun}
                            AND IsDeleted = 0
                        ORDER BY
                            CompanyID ASC
                        ";

                    companyList = connection.Query<SelectListItem>(commandText).ToList();
                }
                return companyList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番重複チェック
        /// </summary>
        /// <param name="product"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static bool IsDuplicateMProduct(M_ProductModel product, string databaseName)
        {
            // 戻り値
            bool isDuplicateValid = false;

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

                    var sql = CreateSQLToSelectDuplicateMProduct(product);

                    int result = Convert.ToInt32(connection.ExecuteScalar(sql));

                    if (result > 0)
                    {
                        isDuplicateValid = true;
                    }
                }
                return isDuplicateValid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 倉庫-品番中間テーブルに存在するかチェック
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static bool IsExistRDepoProduct(int productId, int depoId, string databaseName)
        {
            // 戻り値
            bool isExist = false;

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

                    var sql = CreateSQLToCheckIsExistRDepoProduct(productId, depoId);

                    int result = Convert.ToInt32(connection.ExecuteScalar(sql));

                    if (result > 0)
                    {
                        isExist = true;
                    }
                }
                return isExist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番重複チェック(修正モーダル用)
        /// </summary>
        /// <param name="product">品番情報</param>
        /// <param name="databaseName">データベース名</param>
        public static bool IsDuplicateEditMProduct(M_ProductModel product, string databaseName)
        {
            // 戻り値
            bool isDuplicateValid = false;

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

                    var sql = CreateSQLToSelectDuplicateEditMProduct(product);

                    int result = Convert.ToInt32(connection.ExecuteScalar(sql));

                    if (result > 0)
                    {
                        isDuplicateValid = true;
                    }
                }
                return isDuplicateValid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 品番マスター登録
        /// </summary>
        /// <param name="user"></param>
        /// <param name="databaseName"></param>
        /// <returns>登録結果</returns>
        public static bool InsertMProduct(M_ProductModel product, LoginUserModel loginUser)
        {
            bool result = false;

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
                    DateTime sysDate = DateTime.Now;

                    // 品番マスター登録SQL作成
                    string insertSql = CreateSQLToInsertMProduct(product, sysDate, loginUser.UserName);
                    // 品番マスター登録
                    var insertedProductId = connection.ExecuteScalar(insertSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertedProductId == null)
                    {
                        throw new Exception();
                    }
                    int productId = (int)insertedProductId;

                    // 倉庫-品番中間テーブル登録
                    foreach (SelectListItem item in product.RDepoProductsRegister)
                    {
                        if (item.Selected)
                        {
                            // 倉庫-品番中間テーブル登録SQL作成
                            string depoProductInsertSql = CreateSQLToInsertRDepoProduct(Convert.ToInt32(item.Value), productId, sysDate, loginUser.UserName);
                            int depoProductInsertCount = connection.Execute(depoProductInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (depoProductInsertCount == 0)
                            {
                                throw new Exception();
                            }
                        }
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    result = true;

                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 品番マスター更新
        /// </summary>
        /// <param name="product"></param>
        /// <param name="loginUser"></param>
        /// <returns>更新結果</returns>
        public static bool UpdateMProduct(M_ProductModel product, LoginUserModel loginUser)
        {
            bool result = false;

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
                    DateTime sysDate = DateTime.Now;

                    // 品番マスター更新SQL作成
                    string sql = CreateSQLToUpdateMProduct(product, sysDate, loginUser.UserName);
                    // 品番マスター更新
                    var affectRows = connection.Execute(sql, null, transaction);
                    // 更件数が0の場合はエラーとする
                    if (affectRows == 0)
                    {
                        throw new Exception();
                    }

                    // 倉庫-品番中間テーブル削除SQL作成
                    string depoProductDeleteSql = CreateSQLToDeleteRDepoProduct(product.ProductID);
                    // 倉庫-品番中間テーブル削除
                    int depoProductDelCount = connection.Execute(depoProductDeleteSql, null, transaction);
                    if (depoProductDelCount == 0)
                    {
                        throw new Exception();
                    }

                    // 倉庫-品番中間テーブル更新
                    foreach (SelectListItem item in product.RDepoProductsRegister)
                    {
                        if (item.Selected)
                        {
                            // 倉庫-品番中間テーブル登録SQL作成
                            string depoProductInsertSql = CreateSQLToInsertRDepoProduct(Convert.ToInt32(item.Value), product.ProductID, sysDate, loginUser.UserName);
                            int depoProductInsertCount = connection.Execute(depoProductInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (depoProductInsertCount == 0)
                            {
                                throw new Exception();
                            }
                        }
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    result = true;

                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 品番重複チェックSQL作成
        /// </summary>
        /// <param name="product">品番情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMProduct(M_ProductModel product)
        {
            var sql = $@"
                    SELECT
                        COUNT(*)                      
                    FROM
                        M_Product AS product
                    INNER JOIN M_Company AS supplier ON product.SupplierID = supplier.CompanyID
                    INNER JOIN M_Company AS delivery ON product.DeliveryID = delivery.CompanyID
                    WHERE 
                        (1=1)
	                    AND  
                        (
                            (
		                        supplier.CompanyID = {product.SupplierID}
		                        AND supplier.IsDeleted = 0
		                        AND product.SupplierProductNumber = '{product.SupplierProductNumber}'
	                        )
	                        OR
	                        (
		                        delivery.CompanyID = {product.DeliveryID}
		                        AND delivery.IsDeleted = 0
		                        AND product.DeliveryProductNumber = '{product.DeliveryProductNumber}'
	                        )
                        )
                        AND product.IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なるIDで品番重複チェックSQL作成
        /// </summary>
        /// <param name="product">品番情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMProduct(M_ProductModel product)
        {
            var sql = $@"
                    SELECT
                        COUNT(*)                      
                    FROM
                        M_Product AS product
                    WHERE 
                        (1=1)
                        AND product.ProductID <> {product.ProductID}
	                    AND 
                        (
                            product.SupplierProductNumber = '{product.SupplierProductNumber}'
                            OR product.DeliveryProductNumber = '{product.DeliveryProductNumber}'
                        )
                        AND product.IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        ///  倉庫-品番中間テーブルに存在するかチェックSQL作成
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <param name="depoId">倉庫ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToCheckIsExistRDepoProduct(int productId, int depoId)
        {
            var sql = $@"
                SELECT COUNT(*)  
                FROM R_DepoProduct
                WHERE 
                    DepoID = {depoId}
                    AND ProductID = {productId}
            ";
            return sql;
        }

        /// <summary>
        /// 倉庫-品番中間テーブル登録SQL作成
        /// </summary>
        /// <param name="depoId">登録倉庫ID</param>
        /// <param name="productId">登録品番ID</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertRDepoProduct(int depoId, int productId, DateTime createdAt, string createdBy)
        {
            var sql = $@"
               INSERT INTO R_DepoProduct
                        (DepoID, ProductID, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
               VALUES ({depoId}, {productId}, '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}');
            ";
            return sql;
        }

        /// <summary>
        /// 品番マスター登録SQL作成
        /// </summary>
        /// <param name="product">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMProduct(M_ProductModel product, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO M_Product
                    (SupplierID, SupplierProductNumber, DeliveryID, DeliveryProductNumber, ProductName, LotQuantity, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                OUTPUT INSERTED.ProductID
                VALUES (
                    {product.SupplierID}, '{product.SupplierProductNumber}', {product.DeliveryID}, '{product.DeliveryProductNumber}', '{product.ProductName}', {product.LotQuantity}, '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}'
                )
;
            ";
            return sql;
        }

        /// <summary>
        /// 品番マスター更新SQL作成
        /// </summary>
        /// <param name="product">登録情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMProduct(M_ProductModel product, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Producta
                SET 
                    SupplierID = {product.SupplierID},
                    SupplierProductNumber = '{product.SupplierProductNumber}',
                    DeliveryID = {product.DeliveryID},
                    DeliveryProductNumber = '{product.DeliveryProductNumber}',
                    ProductName = '{product.ProductName}',
                    LotQuantity = {product.LotQuantity},
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    ProductID = {product.ProductID}
            ;";
            return sql;
        }

        // <summary>
        /// 品番マスター削除SQL作成
        /// </summary>
        /// <param name="productId">品番ID</param>
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
        /// 倉庫-品番中間テーブル削除SQL作成
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToDeleteRDepoProduct(int productId)
        {
            var sql = $@"
                DELETE 
                FROM R_DepoProduct
                WHERE ProductID = {productId}
            ";
            return sql;
        }
    }
}
