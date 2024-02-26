using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    public class D_StoreInConnectionController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>商品情報</returns>
        public static List<D_StoreInModel> ConnectDStoreIns(string sql, string databaseName)
        {
            // 戻り値のリスト
            List<D_StoreInModel> list = new List<D_StoreInModel>();

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
                    list = connection.Query<D_StoreInModel>(sql).ToList();
                }

                // 商品情報のリストを返す
                return list;
            }
            catch (Exception)
            {
                // 例外を処理する
                throw;
            }
        }

        /// <summary>
        /// 入庫実績登録
        /// </summary>
        /// <param name="model">入庫実績モデル</param>
        /// <param name="user">ログインユーザー</param>
        public static void InsertDStoreIns(D_StoreInModel model, LoginUserModel user)
        {
            DateTime sysDate = DateTime.Now;

            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(user.DatabaseName);
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
                            item.StoreInDate = Convert.ToDateTime(model.DateSearchStart);

                            // 入庫実績登録SQL作成
                            string insertSql = CreateSQLToInsertDStoreIn(item, sysDate, user.UserName);
                            // 入庫実績登録
                            var insertCount = connection.Execute(insertSql, null, transaction);
                            // 更件数が0の場合はエラーとする
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
        /// 入庫実績削除
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>更新件数</returns>
        public static int DeleteDStoreIn(int storeInId, string databaseName)
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
                    // 入庫実績削除SQL作成
                    string deleteSql = CreateSQLToDeleteDStoreIn(storeInId);
                    // 入庫実績削除
                    int affectedRows = connection.Execute(deleteSql);
                    // 更新件数が0の場合はエラーとする
                    if (affectedRows == 0)
                    {
                        // エラーコード：E2011
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
                    (DepoID, CompanyID, StoreInDate, SupplierProductNumber, LotNumber, MainProductKey, FirstSubProductKey, SecondSubProductKey, NumberOfBoxes, Quantity, Remarks, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES (
                    {model.DepoID}, {model.CompanyID}, '{model.StoreInDate}', '{model.SupplierProductNumber}', '{model.LotNumber}', '{model.MainProductKey}', '{model.FirstSubProductKey}', '{model.SecondSubProductKey}', {model.NumberOfBoxes}, {model.Quantity}, '{model.Remarks}', '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}'
                )
            ;";
            return sql;
        }

        /// <summary>
        /// 入庫実績削除SQL作成
        /// </summary>
        /// <param name="storeInId">入庫実績ID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteDStoreIn(int storeInId)
        {
            var sql = $@"
                        UPDATE D_StoreIn
                        SET IsDeleted = 1
                        WHERE StoreInID = {storeInId}
            ;";
            return sql;
        }

        /// <summary>
        /// 入庫実績情報取得SQL作成
        /// </summary>
        /// <param name="start">入庫日開始</param>
        /// <param name="end">入庫日終了</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetDStoreIns(string start, string end, int depoId, int supplierId)
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
                    ,CASE 
						WHEN storeIn.NumberOfBoxes <> 0 THEN ROUND(storeIn.Quantity / storeIn.NumberOfBoxes, -1, 0)
						ELSE 0
					END AS LotQuantity
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
                WHERE 
	                storeIn.DepoID = {depoId}
                    AND storeIn.CompanyID = {supplierId}
                    AND storeIn.StoreInDate >= '{start}'
                    AND storeIn.StoreInDate <= '{end}'
                    AND storeIn.IsDeleted = 0
                    AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
            ";
            return sql;
        }
    }
}
