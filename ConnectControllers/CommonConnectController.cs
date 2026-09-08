using Microsoft.AspNetCore.Mvc;

namespace shift_pick_management.ConnectControllers
{
    /// <summary>
    /// connectControlerの共通処理
    /// </summary>
    public class CommonConnectController {
        /// <summary>
        /// 選択されたデポを検索条件とするSQLを作成する
        /// </summary>
        /// <param name="checkedDepos">選択されたデポ</param>
        /// <returns></returns>
        public static string SQLOfCheckedDepos(List<string> checkedDepos)
        {
            var sql = "";
            if (checkedDepos.Count > 0)
            {
                sql += "TripHistories.depo_id IN (";
                for (int i = 0; i < checkedDepos.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += $", ";
                    }
                    sql += $"'{checkedDepos[i]}'";
                }
                sql += ")";
            }
            return sql;
        }
    }
}
