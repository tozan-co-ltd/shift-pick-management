using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 品番マスターに関する関数
    /// </summary>
    public class M_ProductConnectController
    {
        /// <summary>
        /// 品番情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_ProductModel> ConnectMProducts(string sql, string databaseName)
        {
            // 戻り値
            List<M_ProductModel> strList = new List<M_ProductModel>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);

                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    strList = connection.Query<M_ProductModel>(sql).ToList();
                }

                return strList;
            }
            catch (Exception)
            {
                throw;
            }
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
                            // 品番-品番中間テーブル情報取得
                            var depoProductSql = CreateSQLToSelectRDepoProducts(item.ProductID);
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
        public static bool IsExistedSupplierProductNumber(string? supplierProductNumber, string databaseName)
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
        /// 納入先品番が品番マスターに存在するかチェック
        /// </summary>
        /// <param name="deliveryProductNumber">納入先品番</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static bool IsExistedDeliveryProductNumber(string? deliveryProductNumber, string databaseName)
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
                            product.DeliveryProductNumber = '{deliveryProductNumber}'
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
        /// <param name="supplierId">仕入先品番</param>
        /// <param name="deliveryProductNumber">納入先品番</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static M_ProductModel? GetProductByDeliveryProductNumber(int supplierId, int depoId, string? deliveryProductNumber, string databaseName)
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
                        FROM M_Product AS product
                        INNER JOIN R_DepoProduct AS depoProduct 
                            ON product.ProductID = depoProduct.ProductID 
                        WHERE 
                            product.SupplierID = {supplierId}
                            AND product.DeliveryProductNumber = '{deliveryProductNumber}'
                            AND depoProduct.DepoId = {depoId}
                            AND product.IsDeleted = 0
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
        /// 出荷の納入先品番から仕入先品番を取得
        /// </summary>
        /// <param name="deliveryId">倉庫ID</param>
        /// <param name="deliveryProductNumber">納入先品番</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static M_ProductModel? GetProductByShipmentDeliveryProductNumber(int deliveryId, int depoId, string? deliveryProductNumber, string databaseName)
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
                        FROM M_Product AS product
                        INNER JOIN R_DepoProduct AS depoProduct 
                            ON product.ProductID = depoProduct.ProductID 
                        WHERE 
                            product.DeliveryID = {deliveryId}
                            AND product.DeliveryProductNumber = '{deliveryProductNumber}'
                            AND depoProduct.DepoId = {depoId}
                            AND product.IsDeleted = 0
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
        /// 仕入先品番から仕入先品番を取得
        /// </summary>
        /// <param name="deliveryId"></param>
        /// <param name="supplierProductNumber"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static M_ProductModel? GetProductBySupplierProductNumber(string supplierProductNumber, string databaseName)
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
                            SupplierProductNumber = '{supplierProductNumber}'
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
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static void DeleteMProduct(int productId, LoginUserModel loginUser)
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
                    DateTime sysDate = DateTime.Now;

                    // 品番履歴テーブル登録SQL作成
                    string logSql = CreateSQLToInsertDProductHistory(productId, "削除", sysDate, loginUser.UserName);
                    var logAddedCount = connection.Execute(logSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (logAddedCount == 0)
                    {
                        throw new Exception();
                    }

                    // 品番マスター削除
                    string productDeleteSql = CreateSQLToDeleteMCompany(productId, sysDate, loginUser.UserName);
                    int productDeleteCount = connection.Execute(productDeleteSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (productDeleteCount == 0)
                    {
                        throw new Exception();
                    }

                    // 品番-品番中間テーブル削除SQL作成
                    string depoProductDeleteSql = CreateSQLToDeleteRDepoProduct(productId);
                    // 品番-品番中間テーブル削除
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
        /// 納入先品番ごとに会社リストを取得
        /// </summary>
        /// <returns></returns>
        public static M_ProductModel GetProductByDeliverProductNUmber(string deliverProductNumber, string databaseName)
        {
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
	                        *
                        FROM M_Product
                        WHERE (1=1)
                            AND DeliveryProductNumber = '{deliverProductNumber}'
                            AND IsDeleted = 0
                        ";

                    var productList = connection.Query<M_ProductModel>(commandText).ToList();
                    if (productList.Count != 1)
                    {
                        throw new Exception();
                    }

                    return productList[0];
                }
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
        /// 品番マスター登録
        /// </summary>
        /// <param name="model"></param>
        /// <param name="loginUser"></param>
        /// <returns>登録結果</returns>
        public static bool InsertMProduct(M_ProductModel model, LoginUserModel loginUser)
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

                    // 品番マスター登録
                    string insertSql = CreateSQLToInsertMProduct(model, sysDate, loginUser.UserName);
                    var insertedProductId = connection.ExecuteScalar(insertSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertedProductId == null)
                    {
                        throw new Exception();
                    }
                    int productId = (int)insertedProductId;

                    // 品番-品番中間テーブル登録
                    foreach (SelectListItem item in model.RDepoProductsRegister)
                    {
                        if (item.Selected)
                        {
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
        /// <param name="model"></param>
        /// <param name="loginUser"></param>
        /// <returns>更新結果</returns>
        public static void UpdateMProduct(M_ProductModel model, LoginUserModel loginUser)
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
                    DateTime sysDate = DateTime.Now;

                    // 品番マスター更新
                    string sql = CreateSQLToUpdateMProduct(model, sysDate, loginUser.UserName);
                    var affectRows = connection.Execute(sql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (affectRows == 0)
                    {
                        throw new Exception();
                    }

                    // 品番-品番中間テーブル削除
                    string depoProductDeleteSql = CreateSQLToDeleteRDepoProduct(model.ProductID);
                    int depoProductDelCount = connection.Execute(depoProductDeleteSql, null, transaction);

                    // 品番-品番中間テーブル登録
                    foreach (SelectListItem item in model.RDepoProductsRegister)
                    {
                        if (item.Selected)
                        {
                            string depoProductInsertSql = CreateSQLToInsertRDepoProduct(Convert.ToInt32(item.Value), model.ProductID, sysDate, loginUser.UserName);
                            int depoProductInsertCount = connection.Execute(depoProductInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (depoProductInsertCount == 0)
                            {
                                throw new Exception();
                            }
                        }
                    }

                    List<SelectListItem> selectedItems = model.RDepoProductsRegister.Where(item => item.Selected).ToList();
                    List<string> selectedValues = selectedItems.Select(item => item.Text).ToList();
                    var depoName = string.Join(",", selectedValues);
                    // 品番履歴テーブル登録SQL作成
                    string logSql = CreateSQLToInsertDProductHistory(model.ProductID, "更新", sysDate, loginUser.UserName, depoName);
                    var logAddedCount = connection.Execute(logSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (logAddedCount == 0)
                    {
                        throw new Exception();
                    }

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
        /// 品番-品番中間テーブル情報取得SQL作成
        /// </summary>
        /// <param name="productId">品番ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRDepoProducts(int productId)
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
        /// 品番重複チェックSQL作成
        /// </summary>
        /// <param name="model">品番情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMProduct(M_ProductModel model)
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
		                        supplier.CompanyID = {model.SupplierID}
		                        AND supplier.IsDeleted = 0
		                        AND product.SupplierProductNumber = '{model.SupplierProductNumber}'
	                        )
	                        OR
	                        (
		                        delivery.CompanyID = {model.DeliveryID}
		                        AND delivery.IsDeleted = 0
		                        AND product.DeliveryProductNumber = '{model.DeliveryProductNumber}'
	                        )
                        )
                        AND product.IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なるIDで重複品番情報取得SQL作成
        /// </summary>
        /// <param name="model">品番情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMProduct(M_ProductModel model)
        {
            var sql = $@"
                    SELECT
                        COUNT(*)                      
                    FROM
                        M_Product AS product
                    WHERE 
                        (1=1)
                        AND product.ProductID <> {model.ProductID}
	                    AND 
                        (
                            product.SupplierProductNumber = '{model.SupplierProductNumber}'
                            OR product.DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        )
                        AND product.IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 倉庫-品番中間テーブルに存在するかチェックSQL作成
        /// </summary>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="productId">品番ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectCheckIsExistRDepoProduct(int depoId, int productId)
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
        /// 品番-品番中間テーブル登録SQL作成
        /// </summary>
        /// <param name="depoId">登録品番ID</param>
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
                UPDATE M_Product
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

        /// <summary>
        /// 品番マスター削除SQL作成
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="updatedAt"></param>
        /// <param name="updatedBy"></param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMCompany(int productId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_Product
                SET 
                    IsDeleted = 1,
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE 
                    ProductID = {productId}
            ;";
            return sql;
        }

        /// <summary>
        /// 品番-品番中間テーブル削除SQL作成
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

        /// <summary>
        /// 品番履歴テーブル登録SQL作成
        /// </summary>
        /// <param name="product">登録情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDProductHistory(M_ProductModel product, string historyStatus, DateTime updatedAt, string updatedBy)
        {
            List<SelectListItem> selectedItems = product.RDepoProductsRegister.Where(item => item.Selected).ToList();
            List<string> selectedValues = selectedItems.Select(item => item.Value).ToList();
            var depoName = string.Join(",", selectedValues);

            var sql = $@"
                INSERT INTO D_ProductHistory
                    (HistoryStatus, DepoName, SupplierName, SupplierProductNumber, DeliveryID, DeliveryProductNumber, ProductName, LotQuantity, UpdatedAt, UpdatedBy)
                VALUES (
                    {historyStatus}, {depoName}, {product.SupplierName}, '{product.SupplierProductNumber}', {product.DeliveryID}, '{product.DeliveryProductNumber}', '{product.ProductName}', {product.LotQuantity}, '{updatedAt}', '{updatedBy}'
                )
;
            ";
            return sql;
        }

        /// <summary>
        /// 品番履歴テーブル登録SQL作成
        /// </summary>
        /// <param name="product">品番ID</param>
        /// <param name="historyStatus">履歴ステータス</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <param name="depoName">デポー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDProductHistory(int productId, string historyStatus, DateTime updatedAt, string updatedBy, string depoName = "")
        {
            var depoNameStr = string.Empty;
            if (string.Empty.Equals(depoName))
            {
                depoNameStr = "COALESCE(STRING_AGG(depo.DepoName,', '), '') AS DepoName";
            }
            else
            {
                depoNameStr = $@"'{depoName}' AS DepoName";
            }
            var sql = $@"
                INSERT INTO D_ProductHistory
                    (HistoryStatus, DepoName, SupplierName, SupplierProductNumber, DeliveryName, DeliveryProductNumber, ProductName, LotQuantity, UpdatedAt, UpdatedBy)
                SELECT 
                    '{historyStatus}', 
                    {depoNameStr}
                    ,supplier.CompanyName as SupplierName
                    ,product.SupplierProductNumber
                    ,delivery.CompanyName
                    ,product.DeliveryProductNumber
                    ,product.ProductName
                    ,product.LotQuantity
                    ,'{updatedAt}'
                    ,'{updatedBy}'
                FROM M_Product product
                LEFT JOIN R_DepoProduct depoProduct ON product.ProductID = depoProduct.ProductID
                LEFT JOIN M_Depo depo ON depoProduct.DepoID = depo.DepoID
                INNER JOIN M_Company supplier ON product.SupplierID = supplier.CompanyID
                INNER JOIN M_Company delivery ON product.DeliveryID = delivery.CompanyID
                WHERE product.ProductID = {productId} AND product.IsDeleted = 0
                GROUP BY 
                    supplier.CompanyName
                    ,product.SupplierProductNumber
                    ,delivery.CompanyName
                    ,product.DeliveryProductNumber
                    ,product.ProductName
                    ,product.LotQuantity
                ;
            ";
            return sql;
        }
    }
}
