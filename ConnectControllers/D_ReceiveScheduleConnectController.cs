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
        /// 入荷予定データ登録
        /// </summary>
        /// <param name="modelList">モデルリスト</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="importFileName">取込ファイル名</param>
        /// <param name="viewTitle">画面名</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        public static bool InsertDReceiveSchedule(List<D_ReceiveScheduleModel> modelList, int depoId, string importFileName, string viewTitle, LoginUserModel loginUser)
        {
            bool insertFlg = false;

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
                    DateTime systemDate = DateTime.Now;

                    foreach (var model in modelList) 
                    {
                        // 入荷予定取込テーブル登録
                        string insertSql = CreateSQLToInsertDReceiveSchedule(model, depoId, systemDate, loginUser.UserName);
                        int affectRows = connection.Execute(insertSql, null, transaction);
                        // 更新件数が0の場合はエラーとする
                        if (affectRows == 0)
                        {
                            throw new Exception();
                        }
                    }

                    // ファイル取込実績テーブル登録
                    D_FileImportModel dFileImportModel = new()
                    {
                        DepoID = depoId,
                        MenuName = viewTitle,
                        ImportFileName = importFileName,
                        CreatedAt = systemDate,
                        CreatedBy = loginUser.UserName
                    };
                    string dFileImportInserSql = D_FileImportConnectController.CreateSQLToInsertDFileImport(dFileImportModel, systemDate, loginUser.UserName);
                    var insertAffectRows = connection.Execute(dFileImportInserSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertAffectRows == 0)
                    {
                        throw new Exception();
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    insertFlg = true;
                    return insertFlg;
                }
                catch (SqlException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 入荷予定情報取得SQL作成
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDReceiveSchedules(D_ReceiveScheduleModel model)
        {
            string differenceCheckCondition = string.Empty;
            if (model.DiffenceCountCheck)
            {
                differenceCheckCondition = " AND (receive_schedule.Quantity / product.LotQuantity) <> COALESCE(storein_sum.StoreInNumberOfBox, 0) ";
            }

            var sql = $@" 
                -- 入荷予定の条件絞り込み
                WITH receive_schedule AS (
                    SELECT 
                        *
                    FROM 
                        D_ReceiveSchedule
                    WHERE 
                        DepoID = {model.SelectedDepoID}
                        AND CompanyID ={model.SelectedCompanyID}
                        AND ReceiveScheduleDate >= '{model.SearchStartDate}'
                        AND ReceiveScheduleDate <= '{model.SearchEndDate}'
                        AND IsDeleted = 0
                ),
                -- 出庫実績の条件絞り込み
                storein_sum AS (
                    SELECT 
                        DepoID,
                        CompanyID,
                        StoreInDate,
                        SupplierProductNumber,
                        LotNumber,
                        SUM(COALESCE(NumberOfBoxes, 0)) AS StoreInNumberOfBox, -- 入庫箱数
                        SUM(COALESCE(Quantity, 0)) AS StoreInQuantity
                    FROM 
                        D_StoreIn
                    WHERE 
                        DepoID ={model.SelectedDepoID}
                        AND StoreInDate >= '{model.SearchStartDate}'
                        AND StoreInDate <= '{model.SearchEndDate}'
                        AND IsDeleted = 0
                    GROUP BY 
                        DepoID,
                        CompanyID,
                        StoreInDate,
                        SupplierProductNumber,
                        LotNumber
                )

                SELECT
                    receive_schedule.ReceiveScheduleID,
                    receive_schedule.DepoID,
                    receive_schedule.CompanyID AS SupplierID,
                    company.CompanyName AS SupplierName,
                    receive_schedule.ReceiveScheduleDate,
                    receive_schedule.SupplierProductNumber,
                    receive_schedule.LotNumber,
                    CASE 
                        WHEN product.LotQuantity <> 0 THEN ROUND(receive_schedule.Quantity / product.LotQuantity, 0)
                        ELSE 0
                    END AS NumberOfBoxes, -- 予定箱数
                    COALESCE(receive_schedule.Quantity, 0) AS Quantity, --予定数量
                    COALESCE(storein_sum.StoreInNumberOfBox, 0) AS StoreInNumberOfBox, -- 入庫箱数
                    COALESCE(storein_sum.StoreInQuantity, 0) AS StoreInQuantity, -- 入庫数量
                    receive_schedule.CreatedAt,
                    receive_schedule.CreatedBy
                FROM 
                    receive_schedule 
                INNER JOIN 
                    M_Product AS product ON receive_schedule.SupplierProductNumber = product.SupplierProductNumber
                LEFT JOIN 
                    storein_sum 
                    ON receive_schedule.DepoID = storein_sum.DepoID
                    AND receive_schedule.CompanyID = storein_sum.CompanyID
                    AND receive_schedule.ReceiveScheduleDate = storein_sum.StoreInDate
                    AND receive_schedule.SupplierProductNumber = storein_sum.SupplierProductNumber
                    AND receive_schedule.LotNumber = storein_sum.LotNumber                    
                INNER JOIN 
                    M_Company AS company ON receive_schedule.CompanyID = company.CompanyID
                WHERE   
                    receive_schedule.IsDeleted = 0
                    AND product.IsDeleted = 0
                    AND company.IsDeleted = 0
                     {differenceCheckCondition}                                      
                ORDER BY 
                    receive_schedule.SupplierProductNumber ASC
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
