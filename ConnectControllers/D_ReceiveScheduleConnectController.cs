using Dapper;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 入荷予定テーブルに関する関数
    /// </summary>
    public static class D_ReceiveScheduleConnectController
    {
        /// <summary>
        /// 入荷予定情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_ReceiveScheduleModel> ConnectDReceiveSchedules(string sql, string databaseName)
        {
            // 戻り値
            List<D_ReceiveScheduleModel> strList = new();

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

                    strList = connection.Query<D_ReceiveScheduleModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 入荷予定SELECT文SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDReceiveSchedules()
        {
            var sql = $@"
                    SELECT
                       *
                    FROM 
                        D_ReceiveSchedule
                    WHERE
                        IsDeleted = 0
                ;";

            return sql;
        }

        /// <summary>
        /// 入荷予定データ書き込み
        /// </summary>
        /// <param name="model"></param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="importFileName">取込ファイル名</param>
        /// <param name="user">ユーザー</param>
        /// <returns></returns>
        public static bool InsertDReceiveSchedule(List<D_ReceiveScheduleModel> modelList, int depoId, string importFileName,　LoginUserModel user)
        {
            bool insertFlg = true;

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
                    DateTime systemDate = DateTime.Now;

                    foreach (var model in modelList) 
                    {
                        // 入荷予定取込SQL作成
                        string insertSql = CreateSQLToInsertDReceiveSchedule(model, depoId, systemDate, user.UserName);
                        // 入荷予定取込
                        int affectRows = connection.Execute(insertSql, null, transaction);
                        // 更新件数が0の場合はエラーとする
                        if (affectRows == 0)
                        {
                            insertFlg = false;
                            // エラーコード：E2011
                            throw new Exception();
                        }
                    }

                    //　ファイル取込実績テーブル
                    D_FileImportModel dFileImportModel = new()
                    {
                        DepoID = depoId,
                        MenuName = "入荷予定取込",
                        ImportFileName = importFileName,
                        CreatedAt = systemDate,
                        CreatedBy = user.UserName
                    };

                    // SQL作成
                    string dFileImportInserSql = D_FileImportConnectController.CreateSQLToInsertDFileImport(dFileImportModel, systemDate, user.UserName);
                    var insertAffectRows = connection.Execute(dFileImportInserSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertAffectRows == 0)
                    {
                        insertFlg = false;
                        // エラーコード：E2011
                        throw new Exception();
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    return insertFlg;
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    insertFlg = false;
                    // エラーコード：E2011
                    throw ex;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    insertFlg = false;
                    // エラーコード：E2011
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 入荷予定情報取得SQL作成
        /// </summary>
        /// <param name="start">入庫日開始</param>
        /// <param name="end">入庫日終了</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="supplierId">会社ID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToGetDReceiveSchedules(string start, string end, int depoId, int supplierId, bool checkFlg)
        {
            string differenceCheckCondition = string.Empty;
            if (checkFlg)
            {
                differenceCheckCondition = " AND (schedule.Quantity / product.LotQuantity) <> storeIn.NumberOfBoxes";
            }

            var sql = $@"
                SELECT
                    schedule.ReceiveScheduleID
                    ,schedule.DepoID
                    ,schedule.CompanyID AS SupplierID
                    ,company.CompanyName AS SupplierName
                    ,schedule.ReceiveScheduleDate
                    ,schedule.SupplierProductNumber
                    ,schedule.LotNumber
                    ,CASE 
	                    WHEN product.LotQuantity <> 0 THEN ROUND(schedule.Quantity / product.LotQuantity, 0, 0)
	                    ELSE 0
                    END AS NumberOfBoxes    -- 予定箱数
                    ,schedule.Quantity      --予定数量
                    ,storeIn.NumberOfBoxes AS StoreInNumberOfBox    -- 入庫箱数
                    ,storeIn.Quantity AS StoreInQuantity            -- 入庫数量
                    ,schedule.CreatedAt
                    ,schedule.CreatedBy
                FROM D_ReceiveSchedule schedule
                INNER JOIN M_Product AS product 
                    ON schedule.SupplierProductNumber = product.SupplierProductNumber
                INNER JOIN D_StoreIn AS storeIn
                    ON schedule.DepoID = storeIn.DepoID
	                AND schedule.CompanyID = storeIn.CompanyID
	                AND schedule.ReceiveScheduleDate = storeIn.StoreInDate
	                AND schedule.SupplierProductNumber = storeIn.SupplierProductNumber
	                AND schedule.LotNumber = storeIn.LotNumber
                INNER JOIN M_Company AS company 
                    ON schedule.CompanyID = company.CompanyID
                WHERE 
                    schedule.DepoID = {depoId}
                    AND schedule.CompanyID = {supplierId}
                    AND schedule.ReceiveScheduleDate >= '{start}'
                    AND schedule.ReceiveScheduleDate <= '{end}'
                    {differenceCheckCondition}
                    AND schedule.IsDeleted = 0
                    AND product.IsDeleted = 0
	                AND storeIn.IsDeleted = 0
	                AND company.IsDeleted = 0
                ORDER BY 
	                schedule.SupplierProductNumber ASC     
            ";
            return sql;
        }

        /// <summary>
        /// 入荷予定データINSERT文SQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="createdAt">システム時間</param>
        /// <param name="userName">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDReceiveSchedule(D_ReceiveScheduleModel model, int depoId, DateTime createdAt,string userName)
        {
            var sql = $@"
            BEGIN
	            IF EXISTS (
		            SELECT 1
		            FROM D_ReceiveSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {model.CompanyID}
                        AND ReceiveScheduleDate = '{model.ReceiveScheduleDate}'
                        AND SupplierProductNumber = '{model.SupplierProductNumber}'
                        AND LotNumber = '{model.LotNumber}'
                        AND IsDeleted = 0
	            )
	            BEGIN

                    UPDATE D_ReceiveSchedule
                    SET Quantity = {model.Quantity}, UpdatedAt = '{createdAt}', UpdatedBy = '{userName}'
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {model.CompanyID}
                        AND ReceiveScheduleDate = '{model.ReceiveScheduleDate}'
                        AND SupplierProductNumber = '{model.SupplierProductNumber}'
                        AND LotNumber = '{model.LotNumber}'
                        AND IsDeleted = 0
	            END
                ELSE
                BEGIN
                    INSERT INTO D_ReceiveSchedule
                    (
                        DepoID,
                        CompanyID,
                        ReceiveScheduleDate,
                        SupplierProductNumber,
                        LotNumber,
                        Quantity,
                        CreatedAt,
                        CreatedBy,
                        UpdatedAt,
                        UpdatedBy
                    )
                    VALUES 
                    (
                        {depoId},
                        {model.CompanyID},
                        '{model.ReceiveScheduleDate}',
                        '{model.SupplierProductNumber}',
                        '{model.LotNumber}',
                        '{model.Quantity}',
                        '{createdAt}',
                        '{userName}',
                        '{createdAt}',
                        '{userName}'
                    );
                END
            END
            ";

            return sql;
        }
    }
}
