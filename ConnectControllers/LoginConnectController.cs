using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;
using System;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// ログインユーザー(ユーザーマスター)に関する関数
    /// </summary>
    public static class LoginConnectController
    {
        /// <summary>
        /// ログインユーザー取得SQL作成
        /// </summary>
        /// <remarks>SELECT文 ユーザーマスターのログインID・管理権限区分が一致するレコード</remarks>
        /// <param name="loginId">ログインID</param>
        /// <returns>SQL</returns>
        public static string CreateSQLToSelectMUserByLoginUser(string loginId)
        {
            var sql = $@"
                    SELECT
                         m_user.UserID
                        ,m_user.LoginID
                        ,m_user.UserName
                        ,m_user.DepoID
                        ,m_depo.DepoName AS MainDepoName
                        ,m_user.AuthorizedKubun
                        ,m_user.Password
                        ,m_user.Salt
                        ,m_user.LastLoginDatetime
                        ,m_user.IsLogin
                        ,m_user.Role
                    FROM 
                        M_User AS m_user
                    INNER JOIN M_Depo AS m_depo ON m_user.DepoID = m_depo.DepoID
                    WHERE
                        m_user.IsDeleted = 0
                        AND m_user.LoginID = '{@loginId}' COLLATE Japanese_CS_AS_KS_WS
            ";

            return sql;
        }

        /// <summary>
        /// 最終ログイン日時が一致するユーザー情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMUserByLastLoginDatetime(int userID, DateTime lastLoginDatetime)
        {
            var sql = $@"
                        SELECT *
                        FROM M_User
                        WHERE (1=1)
                            AND UserID = {userID}
                            AND LastLoginDatetime = '{lastLoginDatetime}'
                            AND IsDeleted = 0
            ;";

            return sql;
        }

        /// <summary>
        /// ログインフラグ・最終ログイン日時更新SQL作成
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLoginDatetime"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToUpdateMUserByLogin(int userId, DateTime lastLoginDatetime)
        {
            var sql = $@"
                            UPDATE M_User
                            SET
                                IsLogin = 1,
                                LastLoginDatetime = '{lastLoginDatetime}'
                            WHERE (1=1)
                                AND UserID = {userId}
            ";
            return sql;
        }

        /// <summary>
        /// ログインフラグ更新SQL作成
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToUpdateMUserByLogout(int userId)
        {
            var sql = $@"
                            UPDATE M_User
                            SET    IsLogin = 0
                            WHERE (1=1)
                                AND UserID = {userId}
            ";
            return sql;
        }
    }
}
