using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_UserConnectController 
    {
        /// <summary>
        /// ユーザー情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns></returns>
        public static List<M_UserModel> ConnectMUsers(string sql)
        {
            // 戻り値
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
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ユーザー情報取得用SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMUsers()
        {
            var sql = $@"
                SELECT 
                   Users.user_id,
                   Users.ad_name,
                   Users.depo_id,
                   Depos.name AS depo_name,
                   Users.authorized_kubun,
                   Users.created_at,
                   Users.created_by,
                   Users.updated_at,
                   Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
            ";
            return sql;
        }
    }
}
