using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 入庫実績テーブルに関する関数
    /// </summary>
    public class D_StoreInConnectController
    {
        /// <summary>
        /// 入庫実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_StoreInModel> ConnectDStoreIns(string sql, string databaseName)
        {
            // 戻り値
            List<D_StoreInModel> strList = new List<D_StoreInModel>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    strList = connection.Query<D_StoreInModel>(sql).ToList();
                }

                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 入庫実績登録
        /// </summary>
        /// <param name="model">入庫実績モデル</param>
        /// <param name="loginUser">ログインユーザー</param>
        public static void InsertDStoreIns(D_StoreInModel model, LoginUserModel loginUser)
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
                        foreach(var item in model.RegisterList)
                        {
                            item.DepoID = model.SelectedDepoID;
                            item.CompanyID = model.SelectedCompanyID;
                            item.StoreInDate = Convert.ToDateTime(model.SearchStartDate);
                            item.NumberOfBoxes = (int)Math.Ceiling((double)item.Quantity / item.LotQuantity);

                            // 入庫実績登録
                            string insertSql = CreateSQLToInsertDStoreIn(item, sysDate, loginUser.UserName);
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
        /// 入庫実績更新
        /// </summary>
        /// <param name="model">入庫実績モデル</param>
        /// <param name="loginUser">ログインユーザー</param>
        public static void UpdateDStoreIn(D_StoreInModel model, LoginUserModel loginUser)
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
                    DateTime sysDate = DateTime.Now;

                    // 入庫実績更新
                    string editSql = CreateSQLToUpdateDStoreIn(model, sysDate, loginUser.UserName);
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
        /// 入庫実績削除
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>更新件数</returns>
        public static int DeleteDStoreIn(int storeInId, LoginUserModel loginUser)
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
                    // 入庫実績削除SQL作成
                    string deleteSql = CreateSQLToDeleteDStoreIn(storeInId, DateTime.Now, loginUser.UserName);
                    // 入庫実績削除
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
        /// 入庫実績情報取得SQL作成
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDStoreIns(D_StoreInModel model)
        {
            var sql = $@"
                SELECT 
	                storeIn.StoreInID
                    ,storeIn.DepoID
                    ,storeIn.CompanyID AS SupplierID
                    ,company.CompanyName AS SupplierName
                    ,storeIn.StoreInDate
                    ,storeIn.SupplierProductNumber
                    ,storeIn.LotNumber
                    ,product.LotQuantity
                    ,storeIn.MainProductKey
                    ,storeIn.FirstSubProductKey
                    ,storeIn.SecondSubProductKey
                    ,storeIn.NumberOfBoxes
                    ,storeIn.Quantity
                    ,storeIn.Remarks
                    ,storeIn.CreatedAt
                    ,storeIn.CreatedBy
                    ,storeIn.UpdatedAt
                    ,storeIn.UpdatedBy
                FROM 
	                D_StoreIn AS storeIn
                INNER JOIN M_Company AS company 
                    ON storeIn.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo 
                    ON storeIn.DepoID = depo.DepoID
                INNER JOIN M_Product AS product 
                    ON storeIn.SupplierProductNumber = product.SupplierProductNumber
                WHERE 
	                storeIn.DepoID = {model.SelectedDepoID}
                    AND storeIn.CompanyID = {model.SelectedCompanyID}
                    AND storeIn.StoreInDate >= '{model.SearchStartDate}'
                    AND storeIn.StoreInDate <= '{model.SearchEndDate}'
                    AND storeIn.IsDeleted = 0
                    AND product.IsDeleted = 0
                    AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 入庫実績登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDStoreIn(D_StoreInModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO D_StoreIn
                    (DepoID, 
                    CompanyID, 
                    StoreInDate, 
                    SupplierProductNumber, 
                    LotNumber, 
                    MainProductKey, 
                    FirstSubProductKey, 
                    SecondSubProductKey, 
                    NumberOfBoxes, 
                    Quantity, 
                    Remarks, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                VALUES (
                    {model.DepoID}, 
                    {model.CompanyID}, 
                    '{model.StoreInDate}', 
                    '{model.SupplierProductNumber}', 
                    '{model.LotNumber}', 
                    '{model.MainProductKey}', 
                    '{model.FirstSubProductKey}', 
                    '{model.SecondSubProductKey}', 
                    {model.NumberOfBoxes}, 
                    {model.Quantity}, 
                    '{model.Remarks}', 
                    '{createdAt}', 
                    '{createdBy}', 
                    '{createdAt}', 
                    '{createdBy}')
            ;";
            return sql;
        }

        /// <summary>
        /// 入庫実績更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateDStoreIn(D_StoreInModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE D_StoreIn
                SET 
                    DepoID = {model.DepoID},
                    CompanyID = {model.CompanyID},
                    StoreInDate = '{model.StoreInDate}',
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
                    StoreInID = {model.StoreInID}
                    AND IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 入庫実績削除SQL作成
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteDStoreIn(int storeInId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                        UPDATE D_StoreIn
                        SET 
                            IsDeleted = 1
                            ,UpdatedAt = '{updatedAt}'
                            ,UpdatedBy = '{updatedBy}'
                        WHERE StoreInID = {storeInId}
            ;";
            return sql;
        }
    }
}
