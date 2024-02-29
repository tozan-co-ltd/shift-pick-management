using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using mar_sumaken_web.Commons;
using System.Reflection;

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
        /// 出庫実績情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDStoreOuts(D_StoreOutModel model)
        {
            string dateSearchStart = model.SearchStartDate + " " + "00:00:00.000";
            string dateSearchEnd = model.SearchEndDate + " " + "23:59:59.999";

            var sql = $@"
                        SELECT 
                            *
                        FROM 
	                        D_StoreOut
                        WHERE StoreOutDate >= CONVERT(datetime, '{dateSearchStart}') 
                        AND StoreOutDate <= CONVERT(datetime, '{@dateSearchEnd}');
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
                        foreach (var item in model.RegisterList)
                        {
                            item.DepoID = model.SelectedDepoID;
                            item.CompanyID = model.SelectedCompanyID;
                            item.StoreOutDate = Convert.ToDateTime(model.SearchStartDate);
                            item.DeliveryDate = Convert.ToDateTime(model.SearchDeliveryDate);
                            item.DeliveryTimeClass = model.SelectedBin;
                            item.DeliverySlipNumber = "Get from bin value";
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
    }
}
