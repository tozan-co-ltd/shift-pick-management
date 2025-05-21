using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.ConnectControllers
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
