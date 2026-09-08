using shift_pick_management.Commons;
using shift_pick_management.Models;

namespace shift_pick_management.ConnectControllers
{
    public class NonDepartedAtRecordConnectController
    {  
        
        public static List<NonDepartedAtRecordModel> GetNonDepartedAtRecords(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepo)
        {
            var sql = CreateSQLToSelectDepartedAtIsNull(startOfPeriod, endOfPeriod, checkedDepo);
            var nonDepartedAtRecords = ConnectToSQLServer.ExecuteQueryToList<NonDepartedAtRecordModel>(sql);
            return nonDepartedAtRecords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <param name="checkedDepo"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectDepartedAtIsNull(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepo)
        {
            string formatStartOfPeriod = startOfPeriod.ToString("yyyy/MM/dd");
            string formatEndOfPeriod = endOfPeriod.ToString("yyyy/MM/dd");
            var sql = $@"
                SELECT
                    a.work_day
                    ,a.depo_id
                    ,Depos.name AS depo_name
                    ,b.null_count
                    ,c.no_identify_number_null_count
	                ,d.trip_count
                FROM 
                    (SELECT 
                        work_day
                        ,depo_id
                    FROM t_trip_records AS a1
                    INNER JOIN m_stations AS a2
                    ON a1.station_id = a2.station_id
                    GROUP BY work_day, depo_id
                    )AS a
                LEFT OUTER JOIN
                    (SELECT
                        work_day
                        ,depo_id
                        ,COUNT(trip_record_id) AS null_count
                    FROM t_trip_records AS b1
                    INNER JOIN m_stations AS b2
                    ON b1.station_id = b2.station_id
                    WHERE departed_at IS NULL
                    AND b1.trip_id IS NOT NULL
                    GROUP BY work_day, depo_id
                    ) AS b
                ON a.work_day = b.work_day
                AND a.depo_id = b.depo_id
                LEFT OUTER JOIN
                    (SELECT
                        work_day
                        ,depo_id
                        ,COUNT(trip_record_id) AS no_identify_number_null_count
                    FROM t_trip_records AS c1
                    INNER JOIN m_stations AS c2
                    ON c1.station_id = c2.station_id
                    WHERE departed_at IS NULL
                    AND c1.trip_id IS NULL
                    GROUP BY work_day, depo_id
                    ) AS c
                ON a.work_day = c.work_day
                AND a.depo_id = c.depo_id
                LEFT OUTER JOIN
                    (SELECT
                        work_day
                        ,depo_id
                        ,COUNT(trip_record_id) AS trip_count
                    FROM t_trip_records AS d1
                    INNER JOIN m_stations AS d2
                    ON d1.station_id = d2.station_id
                    GROUP BY work_day, depo_id
                    ) AS d
                ON a.work_day = d.work_day
                AND a.depo_id = d.depo_id
                INNER JOIN m_depos AS Depos
                ON a.depo_id = Depos.depo_id
                WHERE a.work_day BETWEEN '{formatStartOfPeriod}' AND '{formatEndOfPeriod}'
            ";
            sql += LoadRecordConnectController.SQLOfCheckedDepos(checkedDepo);
            sql += $@"
                ORDER BY a.work_day
            ";
            return sql;
        }
    }
}
