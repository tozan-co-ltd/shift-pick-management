using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 出庫実績テーブルに関する関数
    /// </summary>
    public static class D_StoreOutConnectController
    {
        /// <summary>
        /// 出庫実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_StoreOutModel> ConnectDStoreOuts(string sql, string databaseName)
        {
            // 戻り値
            List<D_StoreOutModel> strList = new();

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

                    strList = connection.Query<D_StoreOutModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 便リスト取得
        /// </summary>
        /// <param name="deliveryDate">納入指示日</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<SelectListItem> GetDeliveryTimeClassList(string deliveryDate, string databaseName)
        {
            // 戻り値
            List<SelectListItem> strList = new();

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

                    string sql = CreateSQLToSelectDeliveryTimeClassList(deliveryDate);
                    strList = connection.Query<SelectListItem>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 出庫実績情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDStoreOuts(D_StoreOutModel model)
        {
            string dateSearchStart = model.SearchStartDate + " " + "00:00:00.000";
            string dateSearchEnd = model.SearchEndDate + " " + "23:59:59.999";

            var sql = $@"
                SELECT 
	                storeOut.storeOutID
                    ,storeOut.DepoID
                    ,storeOut.CompanyID AS SupplierID
                    ,company.CompanyName AS SupplierName
                    ,storeOut.StoreOutDate
                    ,storeOut.DeliveryDate
                    ,storeOut.DeliveryTimeClass
                    ,storeOut.DeliverySlipNumber
                    ,storeOut.DeliveryProductNumber
                    ,storeOut.SupplierProductNumber
                    ,storeOut.LotNumber
                    ,product.LotQuantity
                    ,storeOut.NumberOfBoxes
                    ,storeOut.Quantity
                    ,storeOut.MainProductKey
                    ,storeOut.FirstSubProductKey
                    ,storeOut.SecondSubProductKey
                    ,storeOut.Remarks
                    ,storeOut.CreatedAt
                    ,storeOut.CreatedBy
                    ,storeOut.UpdatedAt
                    ,storeOut.UpdatedBy
                    ,CONCAT(storeOut.DeliveryTimeClass, '便・', storeOut.DeliverySlipNumber) AS SelectedBin
                FROM 
	                D_storeOut AS storeOut
                INNER JOIN M_Company AS company 
                    ON storeOut.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo 
                    ON storeOut.DepoID = depo.DepoID
                INNER JOIN M_Product AS product 
                    ON storeOut.SupplierProductNumber = product.SupplierProductNumber
                WHERE 
	                storeOut.DepoID = {model.SelectedDepoID}
                    AND storeOut.CompanyID = {model.SelectedCompanyID}
                    AND storeOut.StoreOutDate >= '{dateSearchStart}'
                    AND storeOut.StoreOutDate <= '{dateSearchEnd}'
                    AND storeOut.IsDeleted = 0
                    AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
                    AND product.IsDeleted = 0
                ORDER BY storeOut.SupplierProductNumber
            ";
            return sql;
        }

        /// <summary>
        /// 出庫実績登録
        /// </summary>
        /// <param name="model">出庫実績モデル</param>
        /// <param name="user">ログインユーザー</param>
        public static void InsertDStoreOuts(D_StoreOutModel model, LoginUserModel loginUser)
        {
            DateTime sysDate = DateTime.Now;

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
                    if (model.RegisterList != null && model.RegisterList.Count > 0)
                    {
                        // 便・納品書番号
                        int deliveryTimeClass = 0;
                        string deliverySlipNumber = string.Empty;
                        if (model.SelectedBin != null && model.SelectedBin.Length > 0)
                        {
                            string[] binArr = new string[2];
                            binArr = model.SelectedBin.Split("便・");
                            deliveryTimeClass = binArr[0].Length > 0 ? Convert.ToInt32(binArr[0]) : 0;
                            deliverySlipNumber = binArr[1].Length > 0 ? binArr[1] : string.Empty;
                        }

                        foreach (var item in model.RegisterList)
                        {
                            item.DepoID = model.SelectedDepoID;
                            item.CompanyID = model.SelectedCompanyID;
                            item.StoreOutDate = Convert.ToDateTime(model.SearchStartDate);
                            item.DeliveryDate = Convert.ToDateTime(model.SearchDeliveryDate);
                            item.DeliveryTimeClass = deliveryTimeClass;
                            item.DeliverySlipNumber = deliverySlipNumber;
                            item.NumberOfBoxes = (int)Math.Ceiling((double)item.Quantity / item.LotQuantity);
                            // 出庫実績登録SQL作成
                            string insertSql = CreateSQLToInsertDStoreOut(item, sysDate, loginUser.UserName);
                            // 出庫実績登録
                            var insertCount = connection.Execute(insertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (insertCount == 0)
                            {
                                throw new Exception();
                            }
                        }
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
        /// 出庫実績更新
        /// </summary>
        /// <param name="model">出庫実績モデル</param>
        /// <param name="user">ログインユーザー</param>
        public static void EditDStoreOut(D_StoreOutModel model, LoginUserModel loginUser)
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
                    // 出庫実績更新SQL作成
                    DateTime sysDate = DateTime.Now;
                    model.DepoID = model.SelectedDepoID;
                    model.CompanyID = model.SelectedCompanyID;
                    model.StoreOutDate = Convert.ToDateTime(model.SearchStartDate);
                    model.DeliveryDate = Convert.ToDateTime(model.SearchDeliveryDate);
                    // 便・納品書番号
                    int deliveryTimeClass = 0;
                    string deliverySlipNumber = string.Empty;
                    if (model.SelectedBin != null && model.SelectedBin.Length > 0)
                    {
                        string[] binArr = new string[2];
                        binArr = model.SelectedBin.Split("便・");
                        deliveryTimeClass = binArr[0].Length > 0 ? Convert.ToInt32(binArr[0]) : 0;
                        deliverySlipNumber = binArr[1].Length > 0 ? binArr[1] : string.Empty;
                    }
                    model.DeliveryTimeClass = deliveryTimeClass;
                    model.DeliverySlipNumber = deliverySlipNumber;

                    string editSql = CreateSQLToUpdateDStoreOut(model, sysDate, loginUser.UserName);
                    // 出庫実績更新
                    var editCount = connection.Execute(editSql);
                    // 更新件数が0の場合はエラーとする
                    if (editCount == 0)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 出庫実績削除
        /// </summary>
        /// <param name="storeInId">出庫実績ID</param>
        /// <param name="user">ログインユーザー</param>
        /// <returns>更新件数</returns>
        public static int DeleteDStoreOut(int id, LoginUserModel user)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(user.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                // DB接続
                try
                {
                    // 出庫実績削除SQL作成
                    string deleteSql = CreateSQLToDeleteDStoreOut(id, DateTime.Now, user.UserName);
                    // 出庫実績削除
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
        /// 出庫実績更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="createAt">システムタイム</param>
        /// <param name="createBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateDStoreOut(D_StoreOutModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE D_StoreOut
                SET 
                    DepoID = {model.DepoID},
                    CompanyID = {model.CompanyID},
                    StoreOutDate = '{model.StoreOutDate}',
                    DeliveryDate = '{model.DeliveryDate}',
                    DeliveryTimeClass = {model.DeliveryTimeClass},
                    DeliverySlipNumber = '{model.DeliverySlipNumber}',
                    DeliveryProductNumber = '{model.DeliveryProductNumber}',
                    SupplierProductNumber = '{model.SupplierProductNumber}',
                    LotNumber = '{model.LotNumber}',
                    MainProductKey = '{model.MainProductKey}',
                    FirstSubProductKey = '{model.FirstSubProductKey}',
                    SecondSubProductKey = '{model.SecondSubProductKey}',
                    NumberOfBoxes = {model.NumberOfBoxes},
                    Quantity = {model.Quantity},
                    Remarks = '{model.Remarks}',
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    StoreOutID = {model.StoreOutID}
                    AND IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 出庫実績登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDStoreOut(D_StoreOutModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO D_StoreOut
                    (DepoID, CompanyID, StoreOutDate, DeliveryDate, DeliveryTimeClass, DeliverySlipNumber, DeliveryProductNumber, SupplierProductNumber, LotNumber, MainProductKey, FirstSubProductKey, SecondSubProductKey, NumberOfBoxes, Quantity, Remarks, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES (
                    {model.DepoID}, {model.CompanyID}, '{model.StoreOutDate}', '{model.DeliveryDate}', {model.DeliveryTimeClass}, '{model.DeliverySlipNumber}', '{model.DeliveryProductNumber}', '{model.SupplierProductNumber}', '{model.LotNumber}', '{model.MainProductKey}', '{model.FirstSubProductKey}', '{model.SecondSubProductKey}', {model.NumberOfBoxes}, {model.Quantity}, '{model.Remarks}', '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}'
                )
            ;";
            return sql;
        }

        /// <summary>
        /// 納入指示情報取得
        /// </summary>
        /// <param name="deliveryDate">納入指示日</param>
        /// <returns></returns>
        private static string CreateSQLToSelectDeliveryTimeClassList(string deliveryDate)
        {
            var sql = $@"
                SELECT 
	                DISTINCT CONCAT(DeliveryTimeClass, '便・', DeliverySlipNumber) AS Value, 
	                CONCAT(DeliveryTimeClass, '便・', DeliverySlipNumber) AS Text 
                FROM D_ShipmentSchedule
                WHERE 
	                DeliveryDate = '{deliveryDate}'
	                AND IsDeleted = 0
                ORDER BY 
                    CONCAT(DeliveryTimeClass, '便・', DeliverySlipNumber) ASC;
            ";
            return sql;
        }

        /// <summary>
        /// 出庫実績削除SQL作成
        /// </summary>
        /// <param name="storeInId">出庫実績ID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteDStoreOut(int id, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                        UPDATE D_StoreOut
                        SET 
                            IsDeleted = 1
                            ,UpdatedAt = '{updatedAt}'
                            ,UpdatedBy = '{updatedBy}'
                        WHERE StoreOutID = {id}
            ;";
            return sql;
        }
    }
}
