namespace shift_pick_management.ConnectControllers
{
    public class M_DepoConnectController
    {
        /// <summary>
        /// ユーザーのAD名からデポ情報を取得するSQL生成
        /// </summary>
        /// <param name="ADName">AD名</param>
        /// <returns></returns>
        public static string CreateSQLToSelectDepoFromADName(string ADName)
        {
            var sql = $@"
                SELECT
                    Depos.depo_id,
                    name
	                ,gateway
                    ,Depos.created_at
                    ,Depos.created_by
                    ,Depos.updated_at
                    ,Depos.updated_by
                FROM
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE Users.ad_name = '{ADName}'
                AND is_deleted <> 1
            ";
            return sql;
        }
    }
}
