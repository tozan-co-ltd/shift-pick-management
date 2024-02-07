using System.Data;
using System.Text;

namespace mar_sumaken_web.Commons
{
    public static class Utils
    {
        /// <summary>
        /// CSVフィオルを作る
        /// </summary>
        /// <param name="dtDataTable"></param>
        /// <param name="strFilePath"></param>
        public static void ToCSV(this DataTable dtDataTable, string strFilePath)
        {
            // エンコード設定
            Encoding encoding = Encoding.GetEncoding("Shift_JIS");
            using (StreamWriter sw = new StreamWriter(strFilePath, false, encoding))
            {
                // ヘッダー
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);

                // CSVの内容
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();
                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                //CSVファイルを閉じる
                sw.Close();
            }
        }
    }
}
