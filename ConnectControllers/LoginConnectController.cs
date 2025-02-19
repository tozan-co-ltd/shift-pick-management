using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class LoginConnectController 
    {
        /// <summary>
        /// ユーザーの権限区分情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static int GetAuthorizedKubunFromUserName(string sql)
        {
            // 戻り値 デフォルト値は権限無しの1
            var authorizedKubun = 1;

            List<M_UserModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {

                    connection.ConnectionString = connectionString;
                    connection.Open();
                    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                    strList = connection.Query<M_UserModel>(sql).ToList();
                }
                if(strList.Count > 0)
                {
                    authorizedKubun = strList[0].AuthorizedKubun;
                }

                return authorizedKubun;
            }
            catch (Exception)
            {
                throw;
            }
        }

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
