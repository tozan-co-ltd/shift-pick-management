using Dapper;
using shift_pick_management.Commons;
using shift_pick_management.Models;
using System.Data.SqlClient;

namespace shift_pick_management.ConnectControllers
{
    public class LoginConnectController
    {


        /// <summary>
        /// ユーザー名から権限区分を取得するSQL
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectAuthorizedKubunFromUserName(string userName)
        {
            var sql = $@"
                SELECT
	                authorized_kubun
                FROM 
	                m_users
                WHERE ad_name = '{userName}'
                AND is_deleted <> 1
            ";
            return sql;
        }
    }
}

