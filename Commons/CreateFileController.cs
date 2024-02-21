using mar_sumaken_web.Models;
using System.Data;
using System.Text;

namespace mar_sumaken_web.Commons
{
    public static class CreateFileController
    {
        /// <summary>
        /// CSVファイルを作る
        /// </summary>
        /// <param name="dataTable">データテーブル</param>
        /// <param name="filePath">ファイルパス</param>
        public static void ConvertDataTableToCsv(this DataTable dataTable, string filePath)
        {
            try
            {
                // エンコード設定
                Encoding encoding = Encoding.GetEncoding("Shift_JIS");
                using (StreamWriter sw = new StreamWriter(filePath, false, encoding))
                {
                    // ヘッダー
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        sw.Write(dataTable.Columns[i]);
                        if (i < dataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);

                    // CSVの内容
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
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
                            if (i < dataTable.Columns.Count - 1)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ファイル名作成
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <param name="gamenName">画面名</param>
        /// <returns></returns>
        public static string CreateFileName(SearchConditionModel? searchModel, string gamenName)
        {
            string fileName = string.Empty;
            try
            {
                // SearchConditionListがnullの場合
                if (searchModel == null)
                {
                    return string.Concat(fileName, gamenName, ".csv");
                }

                // 日付
                string start = searchModel.SearchStartDate.Replace("/", "");
                string end = searchModel.SearchEndDate.Replace("/", "");

                string seachDate = start; //入庫日
                if (!gamenName.Equals("日別在庫照会"))
                {
                    seachDate = string.Concat(start, "_", end); //入庫日開始_入庫日終了
                }
                fileName = string.Concat(gamenName, "_", searchModel.DepoName, "_", searchModel.CompanyName, "_", seachDate);

                // 仕入先品番
                if (searchModel.SupplierProductNumber != null)
                {
                    fileName = string.Concat(fileName,"_", searchModel.SupplierProductNumber);
                }

                // 便
                if(searchModel.BinList != null)
                {

                    fileName = string.Concat(fileName, string.Join("_", searchModel.BinList));
                }

                return string.Concat(fileName, ".csv"); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
