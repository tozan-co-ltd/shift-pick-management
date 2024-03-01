using Dapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.Models;
using System.Data.SqlClient;

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
        /// 仕入先かんばん情報登録
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>インサート数</returns>
        public static int InsertMSupplierKanban(M_SupplierKanbanModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToInsertMSupplierKanban(model, sysDate, loginUser.UserName);
                    var insertedCount = connection.Execute(sql);

                    return insertedCount;
                }
                catch (Exception)
                {
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
        public static int UpdateMSupplierKanban(M_SupplierKanbanModel model, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToUpdateMSupplierKanban(model, sysDate, loginUser.UserName);
                    var count = connection.Execute(sql);

                    return count;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 仕入先かんばん情報削除
        /// </summary>
        /// <param name="spplierKanbanId">仕入先かんばんID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMSupplierKanban(int spplierKanbanId, LoginUserModel loginUser)
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUser.DatabaseName);
            // SQLServer接続
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // DB接続
                try
                {
                    DateTime sysDate = DateTime.Now;
                    string sql = CreateSQLToDeleteMSupplierKanban(spplierKanbanId, sysDate, loginUser.UserName);
                    var count = connection.Execute(sql);

                    return count;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 仕入先かんばんマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMSupplierKanbans()
        {
            var sql = $@"
                SELECT 
                    SupplierKanbanID
                    ,DepoName
                    ,CompanyName AS SupplierName
                    ,SupplierKanbanName
                    ,CASE 
                        WHEN AllowedDuplicatesFlag = 0 THEN '0(なし)'
                        WHEN AllowedDuplicatesFlag = 1 THEN '1(あり)'
                        ELSE''
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
                    ,FirstSubProductKeyIndex
                    ,SecondSubProductKeyLength
                    ,SecondSubProductKeyIndex
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
        /// <param name="depoCode">仕入先かんばんコード</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMSupplierKanban(int depoCode)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_SupplierKanban
                WHERE
                    DepoCode = {depoCode}
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
                    DepoCode, 
                    DepoName, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy
                )
                VALUES (
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先かんばんマスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToUpdateMSupplierKanban(M_SupplierKanbanModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE M_SupplierKanban
                SET 
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// 仕入先かんばんマスター削除SQL作成
        /// </summary>
        /// <param name="spplierKanbanId"></param>
        /// <param name="updatedAt"></param>
        /// <param name="updatedBy"></param>
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
                    spplierKanbanId = {spplierKanbanId}
            ;";
            return sql;
        }
    }
}
