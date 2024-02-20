using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
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
                    ,storeIn.CompanyID as SupplierID
                    ,company.CompanyName as SupplierName
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
                WHERE 
	                storeIn.DepoID = {depoId}
                    AND storeIn.CompanyID = {supplierId}
                    AND storeIn.StoreInDate >= '{start}'
                    AND storeIn.StoreInDate <= '{end}'
                    AND storeIn.IsDeleted = 0
                    AND company.IsDeleted = 0
            ";
            return sql;
        }
    }
}
