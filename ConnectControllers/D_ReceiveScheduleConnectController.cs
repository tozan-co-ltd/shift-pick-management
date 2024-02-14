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
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>入荷予定情報</returns>
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
                    D_FileImportModel dFileImportModel = new D_FileImportModel();
                    dFileImportModel.DepoID = depoId;
                    dFileImportModel.MenuName = "入荷予定取込";
                    dFileImportModel.ImportFileName = importFileName;
                    dFileImportModel.CreatedAt = systemDate;
                    dFileImportModel.CreatedBy = user.UserName;

                    // SQL作成
                    string dFileImportInserSql = D_FileImportConnectController.CreateSQLToInsertD_FileImport(dFileImportModel, systemDate, user.UserName);
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
                catch (Exception)
                {
                    transaction.Rollback();
                    insertFlg = false;
                    // エラーコード：E2011
                    throw;
                }
            }
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
	            DECLARE @CompanyID AS int;
	            SET @CompanyID = (SELECT TOP 1 CompanyID FROM M_Company WHERE CompanyCode = '{model.CompanyCode}' );

	            IF EXISTS (
		            SELECT 1
		            FROM D_ReceiveSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = @CompanyID
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
                        AND CompanyID = @CompanyID
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
                        @CompanyID,
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
