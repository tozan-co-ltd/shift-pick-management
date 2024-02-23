using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 入庫実績テーブルに関する関数
    /// </summary>
    public class D_StoreInConnectionController
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
                    ,ROUND(storeIn.Quantity / storeIn.NumberOfBoxes, -1, 0) AS LotQuantity
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
