using Dapper;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// ユーザーマスターに関する関数
    /// </summary>
    public static class M_UserConnectController
    {
        /// <summary>
        /// ユーザー情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<M_UserModel> ConnectMUsers(string sql, string databaseName)
        {
            // 戻り値
            List<M_UserModel> strList = new();

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

                    strList = connection.Query<M_UserModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 最終ログイン日時が一致するユーザー情報を取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static bool ConnectMUserWithMatchingLastLoginDatetime(string sql, string databaseName)
        {
            // 戻り値
            bool isMatched = false;

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

                    var strList = connection.Query<M_UserModel>(sql).ToList();

                    if (strList.Count != 0)
                    {
                        isMatched = true;
                    }
                }
                return isMatched;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーマスターの詳細を取得
        /// </summary>
        /// <param name="userList">ユーザー情報</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>ユーザー情報</returns>
        public static List<M_UserModel> GetMUserDetailList(List<M_UserModel> userList, string databaseName)
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

                    if (userList.Count > 0)
                    {
                        foreach (M_UserModel user in userList)
                        {
                            // 倉庫マスター情報取得
                            var userDepoSql = CreateSQLToSelectRUserDepoList(user.UserID);
                            List<M_DepoModel> depoList = connection.Query<M_DepoModel>(userDepoSql).ToList();
                            if(depoList.Count > 0)
                            {
                                user.M_DepoList = depoList;
                            }

                            // ハンディメニューマスター情報取得
                            var userHandyMenuSql = CreateSQLToSelectRUserHandyMenuList(user.UserID);
                            List<M_HandyMenuModel> handyMenuList = connection.Query<M_HandyMenuModel>(userHandyMenuSql).ToList();
                            if (handyMenuList.Count > 0)
                            {
                                user.M_HandyMenuList = handyMenuList;
                            }
                        }
                    }
                }
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーの使用倉庫情報取得
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static List<M_DepoModel> GetUserDepoByUserId(int userId, string databaseName)
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

                    // 倉庫マスター情報取得
                    var userDepoSql = CreateSQLToSelectRUserDepoList(userId);
                    List<M_DepoModel> depoList = connection.Query<M_DepoModel>(userDepoSql).ToList();
                    return depoList; 
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーマスター削除
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>更新件数</returns>
        public static int DeleteMUser(int userId, LoginUserModel loginUser)
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
                    // ユーザーマスター削除
                    string userDeleteSql = CreateSQLToDeleteMUser(userId, DateTime.Now, loginUser.UserName);
                    deleteAffectedRows = connection.Execute(userDeleteSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if(deleteAffectedRows == 0)
                    {
                        throw new Exception();
                    }

                    // ユーザー-倉庫中間テーブル削除
                    string userDepoDeleteSql = CreateSQLToDeleteRUserDepo(userId);
                    connection.Execute(userDepoDeleteSql, null, transaction);

                    // ユーザー-ハンディメニュー中間テーブル削除
                    string userMenuDeleteSql = CreateSQLToDeleteRUserHandyMenu(userId);
                    connection.Execute(userMenuDeleteSql, null, transaction);

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
        /// 重複ユーザー情報取得をチェック
        /// </summary>
        /// <param name="loginId">ログインID</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns>重複結果</returns>
        public static bool CheckIsDuplicateMUserByLoginId(string loginId, string databaseName)
        {
            // 戻り値
            bool isDuplicateValid = false;

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

                    var sql = CreateSQLToSelectDuplicateMUser(loginId);

                    int result = Convert.ToInt32(connection.ExecuteScalar(sql));

                    if (result > 0)
                    {
                        isDuplicateValid = true;
                    }
                }
                return isDuplicateValid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザーマスター登録
        /// </summary>
        /// <param name="mUserModel"></param>
        /// <param name="loginUserModel"></param>
        /// <returns>登録結果</returns>
        public static bool InsertMUser(M_UserModel mUserModel, LoginUserModel loginUserModel)
        {
            bool result = true;
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUserModel.DatabaseName);
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

                    // ユーザーマスター登録SQL作成
                    string userRegisterSql = CreateSQLToInsertMUser(mUserModel, sysDate, loginUserModel.UserName);
                    // ユーザーマスター登録
                    var insertedUserId = connection.ExecuteScalar(userRegisterSql, null, transaction);
                    // 更件数が0の場合はエラーとする
                    if (insertedUserId == null)
                    {
                        result = false;
                        throw new Exception();
                    }

                    int userId = (int) insertedUserId;
                    // ユーザー倉庫中間テーブル登録
                    foreach (SelectListItem depo in mUserModel.DepoSelectList)
                    {
                        if (depo.Selected)
                        {
                            // ユーザー倉庫中間テーブル登録SQL作成
                            string userDepoInsertSql = CreateSQLToInsertRUserDepo(userId, Convert.ToInt32(depo.Value), sysDate, loginUserModel.UserName);
                            int depoInsertCount = connection.Execute(userDepoInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (depoInsertCount == 0)
                            {
                                result = false;
                                throw new Exception();
                            }
                        }
                    }

                    // ユーザーハンディメニュー中間テーブル登録
                    foreach (SelectListItem menu in mUserModel.HandyMenuSelectList)
                    {
                        if(menu.Selected)
                        {
                            // ユーザーハンディメニュー中間テーブル登録SQL作成
                            string userMenuInsertSql = CreateSQLToInsertRUserHandyMenu(userId, Convert.ToInt32(menu.Value), sysDate, loginUserModel.UserName);
                            int menuInsertCount = connection.Execute(userMenuInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (menuInsertCount == 0)
                            {
                                result = false;
                                throw new Exception();
                            }
                        }
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// ユーザーマスターを更新
        /// </summary>
        /// <param name="mUserModel">更新ユーザー情報</param>
        /// <param name="loginUserModel">ログインユーザー情報</param>
        /// <returns></returns>
        public static async Task<bool> UpdateMUser(M_UserModel mUserModel, LoginUserModel loginUserModel)
        {
            bool result = true;
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(loginUserModel.DatabaseName);
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

                    // ユーザーマスター更新
                    string userUpdateSql = CreateSQLToUpdateMUser(mUserModel, sysDate, loginUserModel.UserName);
                    var updatedRows = connection.Execute(userUpdateSql, null, transaction);
                    // 更新件数が0の場合はエラーとする
                    if (updatedRows == 0)
                    {
                        result = false;
                        throw new Exception();
                    }

                    // ユーザー倉庫中間テーブル登録
                    // 削除してから新規作成
                    string userDepoDeleteSql = CreateSQLToDeleteRUserDepoByUserId(mUserModel.UserID);
                    await connection.ExecuteAsync(userDepoDeleteSql, null, transaction);
                    foreach (SelectListItem depo in mUserModel.DepoSelectList)
                    {
                        if (depo.Selected)
                        {
                            string userDepoInsertSql = CreateSQLToInsertRUserDepo(mUserModel.UserID, Convert.ToInt32(depo.Value), sysDate, loginUserModel.UserName);
                            int depoInsertCount = connection.Execute(userDepoInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (depoInsertCount == 0)
                            {
                                result = false;
                                throw new Exception();
                            }
                        }
                    }

                    // ユーザーハンディメニュー中間テーブル登録
                    // 削除してから新規作成
                    string userHandyMenuDeleteSql = CreateSQLToDeleteRHandyMenuByUserId(mUserModel.UserID);
                    await connection.ExecuteAsync(userHandyMenuDeleteSql, null, transaction);
                    foreach (SelectListItem menu in mUserModel.HandyMenuSelectList)
                    {
                        if (menu.Selected)
                        {
                            string userMenuInsertSql = CreateSQLToInsertRUserHandyMenu(mUserModel.UserID, Convert.ToInt32(menu.Value), sysDate, loginUserModel.UserName);
                            int menuInsertCount = connection.Execute(userMenuInsertSql, null, transaction);
                            // 更新件数が0の場合はエラーとする
                            if (menuInsertCount == 0)
                            {
                                result = false;
                                throw new Exception();
                            }
                        }
                    }

                    // トランザクションのコミット
                    transaction.Commit();

                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// ユーザーマスターのパスワード更新
        /// </summary>
        /// <param name="model">更新ユーザー情報</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns></returns>
        public static int UpdateMUserPassword(ChangePasswordModel model, LoginUserModel loginUser)
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
                    string sql = CreateSQLToUpdateMUserPassword(model, sysDate, loginUser.UserName);
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
        /// 重複ユーザー情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateMUser(string loginId)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    M_User
                WHERE
                    LoginID = '{loginId}'
                    AND IsDeleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// ユーザーマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMUsers()
        {
            var sql = $@"
                SELECT
                    m_user.UserID                                
                    ,m_user.LoginID                              
                    ,m_user.UserName
                    ,m_user.DepoID
	                ,m_depo.DepoName
                    ,m_user.AuthorizedKubun
                    ,CASE 
                        WHEN m_user.AuthorizedKubun = 1 THEN '管理者'
                        WHEN m_user.AuthorizedKubun = 2 THEN '作業者'
                        WHEN m_user.AuthorizedKubun = 3 THEN '作業者(解除要)'
                        ELSE''
                    END AS AuthorizedKubunName
                    ,m_user.UpdatedAt
                    ,m_user.UpdatedBy                            
                FROM 
                    M_User AS m_user
                INNER JOIN M_Depo AS m_depo ON 
                    m_user.DepoID = m_depo.DepoID
                WHERE
                    m_user.IsDeleted = 0
                    AND m_depo.IsDeleted = 0
            ;";

            return sql;
        }

        /// <summary>
        /// IDが一致するユーザー情報取得SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        public static string CreateSQLToSelectMUserByUserId(int userId)
        {
            var sql = $@"
                SELECT
                    m_user.UserID                                
                    ,m_user.LoginID                              
                    ,m_user.UserName
                    ,m_user.DepoID
	                ,m_depo.DepoName
                    ,m_user.AuthorizedKubun
                    ,CASE 
                        WHEN m_user.AuthorizedKubun = 1 THEN '管理者'
                        WHEN m_user.AuthorizedKubun = 2 THEN '作業者'
                        WHEN m_user.AuthorizedKubun = 3 THEN '作業者(解除要)'
                        ELSE''
                    END AS AuthorizedKubunName
                    ,FORMAT (m_user.CreatedAt, 'yyyy/MM/dd ') AS CreatedAt
                    ,m_user.CreatedBy
                    ,FORMAT (m_user.UpdatedAt, 'yyyy/MM/dd ') AS UpdatedAt
                    ,m_user.UpdatedBy                            
                FROM 
                    M_User AS m_user
                INNER JOIN M_Depo AS m_depo 
                    ON m_user.DepoID = m_depo.DepoID
                WHERE
                    m_user.UserId = {userId}
                    AND m_user.IsDeleted = 0
                    AND m_depo.IsDeleted = 0
            ;";

            return sql;
        }

        /// <summary>
        /// ユーザー-倉庫中間テーブル情報取得SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRUserDepoList(int userId)
        {
            var sql = $@"
                SELECT 
	                userDepo.DepoID,
	                m_depo.DepoCode,
	                m_depo.DepoName
                FROM 
	                R_UserDepo AS userDepo
                INNER JOIN M_Depo AS m_depo 
                    ON userDepo.DepoID = m_depo.DepoID
                WHERE 
	                userDepo.UserID = {userId}
                    AND m_depo.IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザー-ハンディメニュー中間テーブル情報取得SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectRUserHandyMenuList(int userId)
        {
            var sql = $@"
                SELECT 
	                userMenu.HandyMenuID,
	                menu.HandyMenuName
                FROM 
	                R_UserHandyMenu AS userMenu
                INNER JOIN M_HandyMenu AS menu 
                    ON userMenu.HandyMenuID = menu.HandyMenuID
                WHERE 
	                userMenu.UserID = {userId}
                    AND menu.IsDeleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーハンディメニュー中間テーブル登録SQL作成
        /// </summary>
        /// <param name="userId">登録ユーザーID</param>
        /// <param name="depoId">登録倉庫ID</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertRUserDepo(int userId, int depoId, DateTime createdAt, string createdBy)
        {
            var sql = $@"
               INSERT INTO R_UserDepo 
                        (UserID, DepoID, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
               VALUES ({userId}, {depoId}, '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}');
            ";
            return sql;
        }

        /// <summary>
        /// ユーザー-倉庫中間テーブル登録SQL作成
        /// </summary>
        /// <param name="userId">登録ユーザーI</param>
        /// <param name="handyMenuId">登録ハンディメニューID</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertRUserHandyMenu(int userId, int handyMenuId, DateTime createdAt, string createdBy)
        {
            var sql = $@"
               INSERT INTO R_UserHandyMenu 
                        (UserID, HandyMenuID, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
               VALUES ({userId}, {handyMenuId}, '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}');
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター登録SQL作成
        /// </summary>
        /// <param name="mUser">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertMUser(M_UserModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO M_User 
                    (LoginID, UserName, DepoID, AuthorizedKubun, Password, Salt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                OUTPUT INSERTED.UserID
                VALUES ('{model.LoginID}', '{model.UserName}', {model.DepoID}, {model.AuthorizedKubun}, '{model.Password}', '{model.Salt}', '{createdAt}', '{createdBy}', '{createdAt}', '{createdBy}');
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToUpdateMUser(M_UserModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"

                UPDATE M_User
                SET 
                    UserName = '{model.UserName}',
                    DepoID = {model.DepoID},
                    AuthorizedKubun = {model.AuthorizedKubun},
                    {(string.IsNullOrEmpty(model.Password) ? "" : $"Password = '{model.Password}',")}
                    {(string.IsNullOrEmpty(model.Password) ? "" : $"Salt = '{model.Salt}',")}
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    UserId = {model.UserID}; 
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスターパスワード更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToUpdateMUserPassword(ChangePasswordModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"

                UPDATE M_User
                SET 
                    Password = '{model.Password}',
                    Salt = '{model.Salt}',
                    UpdatedAt = '{updatedAt}',
                    UpdatedBy = '{updatedBy}'
                WHERE
                    UserId = {model.UserID}; 
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター削除SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToDeleteMUser(int userId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                         UPDATE M_User
                         SET
                            IsDeleted = 1
                            ,UpdatedAt = '{updatedAt}'
                            ,UpdatedBy = '{updatedBy}'
                         WHERE 
	                            UserID = {userId}
            ";
            return sql;
        }

        /// <summary>
        /// ユーザー-倉庫中間テーブル削除SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToDeleteRUserHandyMenu(int userId)
        {
            var sql = $@"
                         DELETE 
                            FROM    R_UserHandyMenu
                            WHERE 
	                                UserID = {userId}
            ";
            return sql;
        }

        /// <summary>
        /// ユーザー-ハンディメニュー中間テーブル削除SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToDeleteRUserDepo(int userId)
        {
            var sql = $@"
                         DELETE 
                            FROM    R_UserDepo
                            WHERE 
	                                UserID = {userId}
            ";
            return sql;
        }

        /// <summary>
        ///  ユーザー-倉庫中間テーブル削除SQL作成
        /// </summary>
        /// <param name="userID">更新ユーザーID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteRUserDepoByUserId(int userID)
        {
            var sql = $@"
               DELETE FROM R_UserDepo WHERE UserID = {userID};
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーハンディメニュー中間テーブル削除SQL作成
        /// </summary>
        /// <param name="userID">更新ユーザーID</param>
        /// <returns>SQL文</returns>
        private static string CreateSQLToDeleteRHandyMenuByUserId(int userID)
        {
            var sql = $@"
               DELETE FROM R_UserHandyMenu WHERE UserID = {userID};
            ";
            return sql;
        }
    }
}
