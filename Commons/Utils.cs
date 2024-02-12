using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace mar_sumaken_web.Commons
{
    public static class Utils
    {
        public readonly static int Const_Customer_ID = 1; // 得意先
        public readonly static int Const_Supplier_ID = 2; // 仕入先
        public readonly static int Const_Delivery_ID = 3; // 納入先

        /// <summary>
        /// 会社区分リスト
        /// </summary>
        public readonly static List<SelectListItem> Const_Company_Kubun_List = new List<SelectListItem>()
        {
            new SelectListItem() { Value = "1", Text = "得意先", Selected = false },
            new SelectListItem() { Value = "2", Text = "仕入先", Selected = false },
            new SelectListItem() { Value = "3", Text = "納入先", Selected = false }
        };

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

        /// <summary>
        /// プロパティの表示名を取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>表示名</returns>
        public static string GetDisplayName<T>(string propertyName)
        {
            var displayName = string.Empty;
            var property = typeof(T).GetProperty(propertyName);
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    displayName = displayAttribute.Name;
                }
            }
            return displayName;
        }

        /// <summary>
        /// CSV拡張子チェック
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public static bool IsCsvFile(string fileName)
        {
            if (fileName.Length > 0)
            {
                // ファイル形式チェック
                var extension = Path.GetExtension(fileName);
                if (extension == ".csv")
                {
                    return true;
                };
            }
            return false;
        }

        /// <summary>
        /// CSVファイル保存 
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="categoryName">保存フォルダー</param>
        /// <returns>取込ファイルパス</returns>
        public static string CreateImportFilePath(string fileName,string categoryName)
        {
            try
            {
                // ファイル名(日付_ファイル名)
                var tmpFileName = string.Concat(
                    DateTime.Now.ToString("yyyyMMddHHmmssfff"), "_", Path.GetFileName(fileName)
                );

                // フォルダパス
                var rootPath = Directory.GetCurrentDirectory();
                var folderPath = Path.Combine(rootPath, string.Concat(@"wwwroot\UploadFiles\", categoryName));

                // フォルダが存在しない場合は新規作成
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 取込ファイルパス
                return Path.Combine(folderPath, tmpFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// フィールを読みます。
        /// </summary>
        /// <param name="csvFilePath">csvファイルのパス</param>
        /// <param name="itemCount">項目数</param>
        /// <returns>配列</returns>
        public static List<string[]> ReadCsvFile(string csvFilePath, int itemCount)
        {
            //リスト型の初期化と宣言
            List<string[]> csvLines = new List<string[]>();
            // Encoding.RegisterProviderをShift JISを扱う前にコールする
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //CSVを読み込みモードで開く,文字種はsjis,※代表的なものでいうとUTF-8
            using (StreamReader reader = new StreamReader(csvFilePath, Encoding.GetEncoding("Shift_JIS")))
            {
                while (!reader.EndOfStream)
                {
                    // CSVファイルの一行を読み込む
                    string line = reader.ReadLine();
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    string[] lineValues = line.Split(',');
                    //strに格納
                    csvLines.Add(lineValues.Take(itemCount).ToArray());
                }

                //CSVファイルを閉じる
                reader.Close();
            }
            return csvLines;
        }

        /// <summary>
        /// CSVファイルデータチェック
        /// </summary>
        /// <param name="lines">CSV行配列</param>
        /// <param name="headerList">ヘッダー名リスト</param>
        public static bool CheckCsvData(List<string> lines, List<string> headerList)
        {
            // 配列の行がnullまたは要素がない場合
            if (lines == null || lines.Count == 0)
                return false;

            // ヘッダーリストがnullまたは要素がない場合
            if (headerList == null || headerList.Count == 0)
                return false;

            // 最初の行（ヘッダー）をチェックする
            var header = lines.First();
            if (string.IsNullOrWhiteSpace(header))
                return false;

            // ヘッダーをカンマで分割する
            var columns = header.Split(',');

            // ヘッダーが存在しないか、列数が少なすぎる場合
            if (columns == null || columns.Length != headerList.Count)
                return false;

            // ヘッダー内の列名がheaderListと一致しない場合
            for (int i = 0; i < columns.Length; i++)
            {
                if (!string.Equals(columns[i], headerList[i]))
                    return false;
            }

            // データ行が存在しない場合、falseを返す
            if (lines.Count == 1)
                return false;

            // 各データ行の列数がヘッダーと一致しない場合、falseを返す
            foreach (var line in lines.Skip(1)) // ヘッダーをスキップして2行目からチェックする
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue; // 空行をスキップする

                var data = line.Split(',');
                if (data.Length != columns.Length)
                    return false; // 列数がヘッダーと一致しない場合、falseを返す
            }

            // エラーがない場合、データは有効であると見なす
            return true;
        }

    }
}
