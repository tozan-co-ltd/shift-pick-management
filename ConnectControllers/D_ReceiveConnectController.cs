using Dapper;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 入荷実績テーブルに関する関数
    /// </summary>
    public static class D_ReceiveConnectController
    {
        /// <summary>
        /// 入荷実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_ReceiveModel> ConnectDReceives(string sql, string databaseName)
        {
            // 戻り値
            List<D_ReceiveModel> strList = new();

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

                    strList = connection.Query<D_ReceiveModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 入荷実績情報取得SQL作成
        /// </summary>
        /// <param name="start">入庫日開始</param>
        /// <param name="end">入庫日終了</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetDReceives(string start, string end, int depoId, int supplierId)
        {
            end = string.Concat(end, " 23:59:59");
            var sql = $@"
                SELECT
                    dReceive.ReceiveID
                    ,dReceive.ScanResultID
                    ,dReceive.CompanyID AS SupplierID
                    ,company.CompanyName AS SupplierName
                    ,dReceive.ReceiveDatetime
                    ,dReceive.SupplierProductNumber
                    ,dReceive.LotNumber
                    ,dReceive.MainProductKey
                    ,dReceive.FirstSubProductKey
                    ,dReceive.SecondSubProductKey
                    ,dReceive.NumberOfBoxes
                    ,dReceive.Quantity
                    ,dReceive.ScanedAt
                    ,dReceive.CreatedAt
                    ,dReceive.CreatedBy
                    ,depo.DepoName AS DepoName
                    ,scan.HandyMenuID
                    ,scan.SupplierKanbanID
                    ,scan.NumberOfInputBoxes
                    ,scan.FirstScanedString
                    ,scan.SecondScanedString
                    ,scan.CreatedBy AS ScanCreatedBy
                FROM D_Receive dReceive
                INNER JOIN D_ScanResult AS scan 
	                ON dReceive.ScanResultID = scan.ScanResultID
                INNER JOIN M_Company AS company 
                        ON dReceive.CompanyID = company.CompanyID
                INNER JOIN M_Depo AS depo 
                    ON scan.DepoID = depo.DepoID
                WHERE 
                    scan.DepoID = {depoId}
                    AND dReceive.CompanyID = {supplierId}
                    AND dReceive.ReceiveDatetime >= '{start}'
                    AND dReceive.ReceiveDatetime <= '{end}'
	                AND company.IsDeleted = 0
                    AND depo.IsDeleted = 0
                ORDER BY 
	                dReceive.SupplierProductNumber ASC 
            ";
            return sql;
        }
    }
}
