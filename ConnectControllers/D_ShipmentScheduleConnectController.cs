using Dapper;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// 出荷指示テーブルに関する関数
    /// </summary>
    public static class D_ShipmentScheduleConnectController
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>出荷指示情報</returns>
        public static List<D_ShipmentScheduleModel> ConnectDShipmentSchedules(string sql, string databaseName)
        {
            // 戻り値
            List<D_ShipmentScheduleModel> strList = new();

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

                    strList = connection.Query<D_ShipmentScheduleModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社マスターSELECT文SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDShipmentSchedules()
        {
            var sql = $@"
                    SELECT
                       *
                    FROM 
                        D_ShipmentSchedule
                    WHERE
                        IsDeleted = 0
                ;";

            return sql;
        }

        /// <summary>
        /// 出荷指示データ書き込み
        /// </summary>
        /// <param name="model"></param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="importFileName">取込ファイル名</param>
        /// <param name="user">ユーザー</param>
        /// <returns></returns>
        public static bool InsertDShipmentSchedule(List<D_ShipmentScheduleModel> modelList, int depoId, int companyId, string importFileName,　LoginUserModel user)
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
                        // 出荷指示取込SQL作成
                        string insertSql = CreateSQLToInsertDShipmentSchedule(model, depoId, companyId, systemDate, user.UserName);
                        // 出荷指示取込
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
                    dFileImportModel.MenuName = "出荷指示取込";
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
        /// 出荷指示データINSERT文SQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="depoId">倉庫ID</param>
        /// <param name="companyId">会社ID</param>
        /// <param name="userName">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertDShipmentSchedule(D_ShipmentScheduleModel model, int depoId, int companyId, DateTime createdAt,string userName)
        {
            var sql = $@"

            BEGIN 
	            DECLARE @SupplierProductNumber AS nvarchar(100);
	            SET @SupplierProductNumber = (SELECT TOP 1 SupplierProductNumber FROM M_Product WHERE DeliveryProductNumber = '{model.DeliveryProductNumber}' );
                IF @SupplierProductNumber IS NULL
		        BEGIN
			        RAISERROR('表示用品番は正しくありません。', 16, 1)
			        RETURN;
		        END                

	            IF EXISTS (
		            SELECT 1
		            FROM D_ShipmentSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {companyId}
                        AND DeliveryDate = '{model.DeliveryDate}'
                        AND DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        AND IsDeleted = 0
	            )
	            BEGIN
		            DELETE FROM D_ShipmentSchedule
		            WHERE 
                        DepoID = {depoId} 
                        AND CompanyID = {companyId}
                        AND DeliveryDate = '{model.DeliveryDate}'
                        AND DeliveryTimeClass = {model.DeliveryTimeClass}
                        AND DeliverySlipNumber = '{model.DeliverySlipNumber}'
                        AND DeliveryProductNumber = '{model.DeliveryProductNumber}'
                        AND IsDeleted = 0
	            END
            
                BEGIN
                    INSERT INTO D_ShipmentSchedule
                    (
                        DepoID,
                        CompanyID,
                        OrdererCode,
                        OrdererFactoryKubun,
                        OrdererName,
                        OrdererFactoryName,
                        ShipperCode,
                        ShipperFactoryKubun,
                        ShipperName,
                        DeliveryCode,
                        DeliveryFactoryKubun,
                        DeliveryLocation,
                        DeliveryName,
                        DeliveryFactoryName,
                        RegularKubun,
                        IssuedDate,
                        DeliveryDate,
                        DeliveryTime,
                        DeliveryTimeClass,
                        TranspotationIdentify,
                        DeliverySlipNumber,
                        DeliverySlipPageNumber,
                        DeliverySlipRowNumber,
                        DeliveryProductNumber,
                        DeliveryProductAbbreviation,
                        DeliveryProductName,
                        LotQuantity,
                        BranchNumber,
                        Quantity,
                        SupplierProductNumber,
                        NumberOfBoxes,
                        CreatedAt,
                        CreatedBy,
                        UpdatedAt,
                        UpdatedBy
                    )
                    VALUES 
                    (
                        {depoId},
                        {companyId},
                        '{model.OrdererCode}',
                        '{model.OrdererFactoryKubun}',
                        '{model.OrdererName}',
                        '{model.OrdererFactoryName}',
                        '{model.ShipperCode}',
                        '{model.ShipperFactoryKubun}',
                        '{model.ShipperName}',
                        '{model.DeliveryCode}',
                        '{model.DeliveryFactoryKubun}',
                        '{model.DeliveryLocation}',
                        '{model.DeliveryName}',
                        '{model.DeliveryFactoryName}',
                        '{model.RegularKubun}',
                        '{model.IssuedDate}',
                        '{model.DeliveryDate}',
                        '{model.DeliveryTime}',
                        {model.DeliveryTimeClass},
                        '{model.TranspotationIdentify}',
                        '{model.DeliverySlipNumber}',
                        {model.DeliverySlipPageNumber},
                        {model.DeliverySlipRowNumber},
                        '{model.DeliveryProductNumber}',
                        '{model.DeliveryProductAbbreviation}',
                        '{model.DeliveryProductName}',
                        {model.LotQuantity},
                        {model.BranchNumber},
                        {model.Quantity},
                        @SupplierProductNumber,
                        {model.NumberOfBoxes},
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
