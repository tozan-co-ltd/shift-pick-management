using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Reflection;

namespace mar_sumaken_web.ConnectControllers
{
    /// <summary>
    /// 仕入先かんばんマスターに関する関数
    /// </summary>
    public static class M_SupplierKanbanConnectController
    {
        /// <summary>
        /// 仕入先かんばん情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_SupplierKanbanModel> ConnectMSupplierKanbans(string sql, string databaseName)
        {
            // 戻り値
            List<M_SupplierKanbanModel> strList = new();

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

                    strList = connection.Query<M_SupplierKanbanModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 仕入先かんばんマスターの詳細を取得
        /// </summary>
        /// <param name="supplierKanbanList">仕入先かんばん情報</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>仕入先かんばん情報</returns>
        public static List<M_SupplierKanbanModel> GetMSupplierKanbanDetailList(List<M_SupplierKanbanModel> supplierKanbanList, string databaseName)
        {
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    if (supplierKanbanList.Count > 0)
                    {
                        foreach (M_SupplierKanbanModel supplierKanban in supplierKanbanList)
                        {
                            // ハンディメニューマスター情報取得
                            var handyMenuSupplierKanbanSql = CreateSQLToSelectRHandyMenuSupplierKanbanList(supplierKanban.SupplierKanbanID);
                            List<M_HandyMenuModel> handyMenuList = connection.Query<M_HandyMenuModel>(handyMenuSupplierKanbanSql).ToList();
                            if (handyMenuList.Count > 0)
                            {
                                supplierKanban.M_HandyMenuList = handyMenuList;
                            }
                        }
                    }
                }
                return supplierKanbanList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 仕入先かんばんマスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>登録結果</returns>
        public static void InsertMSupplierKanban(M_SupplierKanbanModel model, LoginUserModel loginUser)
        {
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
                    DateTime sysDate = DateTime.Now;

                    // 仕入先かんばんマスター登録
                    string supplierKanbanRegisterSql = CreateSQLToInsertMSupplierKanban(model, sysDate, loginUser.UserName);
                    var insertedSupplierKanbanId = connection.ExecuteScalar(supplierKanbanRegisterSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (insertedSupplierKanbanId == null)
                    {
                        throw new Exception();
                    }
                    int supplierKanbanId = (int)insertedSupplierKanbanId;

                    foreach (SelectListItem menu in model.HandyMenuSelectList)
                    {
                        if (menu.Selected)
                        {
                            // ハンディメニュー-仕入先かんばん中間テーブル登録
                            string handyMenuInsertSql = CreateSQLToInsertRHandyMenuSupplierKanban(Convert.ToInt32(menu.Value), supplierKanbanId, sysDate, loginUser.UserName);
                            int menuInsertCount = connection.Execute(handyMenuInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (menuInsertCount == 0)
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
        /// 仕入先かんばん情報更新
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static void UpdateMSupplierKanban(M_SupplierKanbanModel model, LoginUserModel loginUser)
        {
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
                    DateTime sysDate = DateTime.Now;

                    // 仕入先かんばんマスター更新
                    string sql = CreateSQLToUpdateMSupplierKanban(model, sysDate, loginUser.UserName);
                    connection.Execute(sql, null, transaction);

                    // ハンディメニュー-仕入先かんばん中間テーブル削除
                    string handyMenuDeleteSql = CreateSQLToDeleteRHandyMenuSupplierKanban(model.SupplierKanbanID);
                    connection.Execute(handyMenuDeleteSql, null, transaction);

                    // ハンディメニュー-仕入先かんばん中間テーブル登録
                    foreach (SelectListItem menu in model.HandyMenuSelectList)
                    {
                        if (menu.Selected)
                        {
                            // ハンディメニュー-仕入先かんばん中間テーブル登録
                            string handyMenuInsertSql = CreateSQLToInsertRHandyMenuSupplierKanban(Convert.ToInt32(menu.Value), model.SupplierKanbanID, sysDate, loginUser.UserName);
                            int menuInsertCount = connection.Execute(handyMenuInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (menuInsertCount == 0)
                            {
                                throw new Exception();
                            }
                        }
                    }

                    // 仕入先かんばん履歴テーブル登録
                    string logSql = CreateSQLToInsertMSupplierKanbanHistory(model.SupplierKanbanID, "更新", sysDate, loginUser.UserName);
                    connection.Execute(logSql, null, transaction);

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
        /// 仕入先かんばんマスター削除
        /// </summary>
        /// <param name="spplierKanbanId">仕入先かんばんID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMSupplierKanban(int spplierKanbanId, LoginUserModel loginUser)
        {
            int deleteAffectedRows = 0;

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
                    DateTime sysDate = DateTime.Now;
                    // 仕入先かんばんマスター削除
                    string spplierKanbanDeleteSql = CreateSQLToDeleteMSupplierKanban(spplierKanbanId, sysDate, loginUser.UserName);
                    deleteAffectedRows = connection.Execute(spplierKanbanDeleteSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (deleteAffectedRows == 0)
                    {
                        throw new Exception();
                    }

                    // ハンディメニュー-仕入先かんばん中間テーブル削除
                    string handyMenuDeleteSql = CreateSQLToDeleteRHandyMenuSupplierKanban(spplierKanbanId);
                    connection.Execute(handyMenuDeleteSql, null, transaction);

                    // 仕入先かんばん履歴テーブル登録
                    string logSql = CreateSQLToInsertMSupplierKanbanHistory(spplierKanbanId, "削除", sysDate, loginUser.UserName);
                    connection.Execute(logSql, null, transaction);

                    // トランザクションのコミット
                    transaction.Commit();

                    return deleteAffectedRows;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 仕入先かんばんマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMSupplierKanbans(int supplierKanbanId = 0)
        {
            string searchCondition = string.Empty;
            if(supplierKanbanId > 0)
            {
                searchCondition = $@" AND SupplierKanbanID = {supplierKanbanId}";
            }
            var sql = $@"
                SELECT 
                    SupplierKanbanID
                    ,supplierKanban.DepoId
                    ,DepoName
                    ,supplierKanban.CompanyId AS SupplierId
                    ,CompanyName AS SupplierName
                    ,SupplierKanbanName
                    ,CASE 
                        WHEN AllowedDuplicatesFlag = 0 THEN 0
                        WHEN AllowedDuplicatesFlag = 1 THEN 1
                        ELSE ''
                        END AS AllowedDuplicatesFlag
                    ,IdentifyString
                    ,IdentifyStringStartIndex
                    ,ProductNumberStartIndex
                    ,ProductNumberLength
                    ,QuantityLength
                    ,QuantityStartIndex
                    ,LotLength
                    ,LotStartIndex
                    ,MainProductKeyLength
                    ,MainProductKeyStartIndex
                    ,FirstSubProductKeyLength
                    ,FirstSubProductKeyStartIndex
                    ,SecondSubProductKeyLength
                    ,SecondSubProductKeyStartIndex
                    ,ProductBranchNumberLength
                    ,ProductBranchNumberStartIndex
                    ,OrderNumberLength
                    ,OrderNumberStartIndex
                    ,supplierKanban.UpdatedAt
                    ,supplierKanban.UpdatedBy
                FROM 
                    M_SupplierKanban AS supplierKanban
                INNER JOIN M_Depo AS depo 
                    ON supplierKanban.DepoID = depo.DepoID
                INNER JOIN M_Company AS company 
                    ON supplierKanban.CompanyID = company.CompanyID
                WHERE 
                    supplierKanban.IsDeleted = 0
                    {searchCondition}
                    AND depo.IsDeleted = 0
                    AND company.IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ハンディメニュー-仕入先かんばん中間テーブル情報取得SQL作成
        /// </summary>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRHandyMenuSupplierKanbanList(int supplierKanbanId)
        {
            var sql = $@"
                SELECT 
	                handyMenuSupplierKanban.HandyMenuID,
	                menu.HandyMenuName
                FROM 
	                R_HandyMenuSupplierKanban AS handyMenuSupplierKanban
                INNER JOIN M_HandyMenu AS menu 
                    ON handyMenuSupplierKanban.HandyMenuID = menu.HandyMenuID
                WHERE 
	                handyMenuSupplierKanban.SupplierKanbanID = {supplierKanbanId}
                    AND menu.IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 重複仕入先かんばん情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMSupplierKanban(M_SupplierKanbanModel model)
        {
            // 倉庫ID・識別文字・識別文字開始位置が重複していたらエラー
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_SupplierKanban
                WHERE
                    DepoID = {model.SelectedDepoID}
                    AND IdentifyString = '{model.IdentifyString}'
                    AND IdentifyStringStartIndex = {model.IdentifyStringStartIndex}
                    AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 異なるIDで重複仕入先かんばん情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateEditMSupplierKanban(M_SupplierKanbanModel model)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_SupplierKanban
                WHERE
                    IdentifyString = {model.IdentifyString}
                    IdentifyStringStartIndex = {model.IdentifyStringStartIndex}
                    AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 仕入先かんばんマスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMSupplierKanban(M_SupplierKanbanModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO M_SupplierKanban(
                    DepoID, 
                    CompanyID, 
                    SupplierKanbanName, 
                    AllowedDuplicatesFlag, 
                    IdentifyString, 
                    IdentifyStringStartIndex, 
                    ProductNumberLength, 
                    ProductNumberStartIndex, 
                    QuantityLength, 
                    QuantityStartIndex, 
                    LotLength, 
                    LotStartIndex, 
                    MainProductKeyLength, 
                    MainProductKeyStartIndex, 
                    FirstSubProductKeyLength, 
                    FirstSubProductKeyStartIndex, 
                    SecondSubProductKeyLength, 
                    SecondSubProductKeyStartIndex, 
                    ProductBranchNumberLength, 
                    ProductBranchNumberStartIndex, 
                    OrderNumberLength, 
                    OrderNumberStartIndex, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                OUTPUT INSERTED.SupplierKanbanID
                VALUES (
                    {model.SelectedDepoID},
                    {model.SelectedSupplierID},
                    '{model.SupplierKanbanName}',
                    {model.AllowedDuplicatesFlag},
                    '{model.IdentifyString}',
                    {model.IdentifyStringStartIndex},
                    {model.ProductNumberLength},
                    {model.ProductNumberStartIndex},
                    {model.QuantityLength},
                    {model.QuantityStartIndex},
                    {model.LotLength},
                    {model.LotStartIndex},
                    {model.MainProductKeyLength},
                    {model.MainProductKeyStartIndex},
                    {model.FirstSubProductKeyLength},
                    {model.FirstSubProductKeyStartIndex},
                    {model.SecondSubProductKeyLength},
                    {model.SecondSubProductKeyStartIndex},
                    {model.ProductBranchNumberLength},
                    {model.ProductBranchNumberStartIndex},
                    {model.OrderNumberLength},
                    {model.OrderNumberStartIndex},
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// ハンディ-仕入先かんばん中間テーブル登録SQL作成
        /// </summary>
        /// <param name="handyMenuId">登録ハンディメニューID</param>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertRHandyMenuSupplierKanban(int handyMenuId, int supplierKanbanId, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO R_HandyMenuSupplierKanban(
                    HandyMenuID, 
                    SupplierKanbanID, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                VALUES (
                    '{handyMenuId}',
                    '{supplierKanbanId}',
                    '{createdAt}',
                    '{createdBy}',
                    '{createdAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先かんばんマスター更新SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMSupplierKanban(M_SupplierKanbanModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_SupplierKanban
                SET 
                    CompanyID = '{model.SelectedSupplierID}',
                    SupplierKanbanName = '{model.SupplierKanbanName}',
                    AllowedDuplicatesFlag = '{model.AllowedDuplicatesFlag}',
                    IdentifyString = '{model.IdentifyString}',
                    IdentifyStringStartIndex = {model.IdentifyStringStartIndex},
                    ProductNumberStartIndex = {model.ProductNumberStartIndex},
                    ProductNumberLength = {model.ProductNumberLength},
                    QuantityLength = {model.QuantityLength},
                    QuantityStartIndex = {model.QuantityStartIndex},
                    LotLength = {model.LotLength},
                    LotStartIndex = {model.LotStartIndex},
                    MainProductKeyLength = {model.MainProductKeyLength},
                    MainProductKeyStartIndex = {model.MainProductKeyStartIndex},
                    FirstSubProductKeyLength = {model.FirstSubProductKeyLength},
                    FirstSubProductKeyStartIndex = {model.FirstSubProductKeyStartIndex},
                    SecondSubProductKeyLength = {model.SecondSubProductKeyLength},
                    SecondSubProductKeyStartIndex = {model.SecondSubProductKeyStartIndex},
                    ProductBranchNumberLength = {model.ProductBranchNumberLength},
                    ProductBranchNumberStartIndex = {model.ProductBranchNumberStartIndex},
                    OrderNumberLength = {model.OrderNumberLength},
                    OrderNumberStartIndex = {model.OrderNumberStartIndex},
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    SupplierKanbanID = {model.SupplierKanbanID}
                    AND IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先かんばんマスター削除SQL作成
        /// </summary>
        /// <param name="spplierKanbanId">仕入先かんばんID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteMSupplierKanban(int spplierKanbanId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_SupplierKanban
                SET 
                    IsDeleted = 1,
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE 
                    SupplierKanbanID = {spplierKanbanId}
            ";
            return sql;
        }

        /// <summary>
        /// ハンディメニュー-仕入先かんばん中間テーブル削除SQL作成
        /// </summary>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteRHandyMenuSupplierKanban(int supplierKanbanId)
        {
            var sql = $@"
                DELETE 
                FROM R_HandyMenuSupplierKanban
                WHERE SupplierKanbanID = {supplierKanbanId}
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先かんばん履歴テーブル登録SQL作成
        /// </summary>
        /// <param name="supplierKanbanId">仕入先かんばんID</param>
        /// <param name="historyStatus">履歴状態</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToInsertMSupplierKanbanHistory(int supplierKanbanId, string historyStatus, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                INSERT INTO D_SupplierKanbanHistory
                    (HistoryStatus, DepoName, SupplierName, SupplierKanbanName, AllowedDuplicatesFlag, 
                     IdentifyString, IdentifyStringStartIndex, ProductNumberStartIndex, ProductNumberLength, 
                     QuantityLength, QuantityStartIndex, LotLength, LotStartIndex, MainProductKeyLength, 
                     MainProductKeyStartIndex, FirstSubProductKeyLength, FirstSubProductKeyStartIndex, SecondSubProductKeyLength, 
                     SecondSubProductKeyStartIndex, ProductBranchNumberLength, ProductBranchNumberStartIndex, OrderNumberLength, 
                     OrderNumberStartIndex, UpdatedAt, UpdatedBy)
                SELECT 
                    '{historyStatus}', 
                    depo.DepoName,
                    supplier.CompanyName AS SupplierName,
                    supplierKanban.SupplierKanbanName,
                    supplierKanban.AllowedDuplicatesFlag,
                    supplierKanban.IdentifyString,
                    supplierKanban.IdentifyStringStartIndex,
                    supplierKanban.ProductNumberStartIndex,
                    supplierKanban.ProductNumberLength,
                    supplierKanban.QuantityLength,
                    supplierKanban.QuantityStartIndex,
                    supplierKanban.LotLength,
                    supplierKanban.LotStartIndex,
                    supplierKanban.MainProductKeyLength,
                    supplierKanban.MainProductKeyStartIndex,
                    supplierKanban.FirstSubProductKeyLength,
                    supplierKanban.FirstSubProductKeyStartIndex,
                    supplierKanban.SecondSubProductKeyLength,
                    supplierKanban.SecondSubProductKeyStartIndex,
                    supplierKanban.ProductBranchNumberLength,
                    supplierKanban.ProductBranchNumberStartIndex,
                    supplierKanban.OrderNumberLength,
                    supplierKanban.OrderNumberStartIndex,
                    '{updatedAt}',
                    '{updatedBy}'
                FROM M_SupplierKanban AS supplierKanban
                LEFT JOIN 
                    M_Depo AS depo ON supplierKanban.DepoID = depo.DepoID
                INNER JOIN 
                    M_Company AS supplier ON supplierKanban.CompanyID = supplier.CompanyID
                WHERE 
                    supplierKanban.SupplierKanbanID = {supplierKanbanId}
            ";
            return sql;
        }
    }
}
