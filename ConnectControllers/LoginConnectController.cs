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
                         m_user.UserID
                        ,m_user.LoginID
                        ,m_user.UserName
                        ,m_user.DepoID
                        ,m_user.AuthorizedKubun
                        ,m_user.Password
                        ,m_user.Salt
                        ,m_user.LastLoginDatetime
                        ,m_user.IsLogin
                        ,m_depo.DepoName
                    FROM 
                        M_User AS m_user
                    INNER JOIN M_Depo AS m_depo ON m_user.DepoID = m_depo.DepoID
                    WHERE
                        m_user.NotUseFlag = 0
                        AND m_user.LoginID = '{@loginId}' COLLATE Japanese_CS_AS_KS_WS
            ";

            return sql;
        }
    }
}
