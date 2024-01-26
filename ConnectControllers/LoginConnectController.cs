using Dapper;
using System.Data.SqlClient;
using mar_sumaken_web.Models;

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
        public static string CreateSQLToGetLoginUser(string loginId)
        {
            var sql = $@"
                    SELECT
                         U.UserID
                        ,U.LoginID
                        ,U.UserName
                        ,U.DepoID
                        ,U.AuthorizedKubun
                        ,U.Password
                        ,U.Salt
                        ,U.LastLoginDatetime
                        ,U.IsLogin
                        ,depo.DepoName
                    FROM 
                        M_User U
                    INNER JOIN M_Depo depo ON U.DepoID = depo.DepoID
                    WHERE
                        U.NotUseFlag = 0
                        AND U.LoginID = '{@loginId}' COLLATE Japanese_CS_AS_KS_WS
            ";

            return sql;
        }
    }
}
