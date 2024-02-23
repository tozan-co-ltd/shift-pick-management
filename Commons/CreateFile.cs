using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace mar_sumaken_web.Commons
{
    public static class CreateFile
    {
        /// <summary>
        /// CSVファイルデータ読取
        /// </summary>
        /// <param name="csvModel">入力モデル</param>
        /// <param name="gamenName">画面名</param>
        /// <returns></returns>
        public async static Task<(string, List<string[]>?, string)> ReadCsv(CsvFileInputModel csvModel, string gamenName)
        {
            var message = string.Empty;
            var lines = new List<string[]>();

            try
            {
                // CSV拡張子チェック
                if (!IsCsvExtension(csvModel.FileName))
                {
                    return ("E1013: " + ErrorMessagesResources.E1013, null, string.Empty);
                };

                // ファイルコピー
                string importFilePath = await CopyCsvFile(csvModel.ImportFile, gamenName);

                // CSVファイルデータ読み取り
                lines = ReadCsvFile(importFilePath, csvModel.HeaderColumnCount);

                // CSVファイルデータチェック
                bool isValidCsv = CheckCsvData(lines, csvModel.HeaderSettings);
                if (!isValidCsv)
                {
                    return ("E1014: " + ErrorMessagesResources.E1014, null, importFilePath);
                }

                return (message, lines, importFilePath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// DataTableをCSV形式の文字列に変換
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
                    // ヘッダー行作成
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        sw.Write(dataTable.Columns[i]);
                        if (i < dataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);

                    // データ行作成
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
                    sw.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// CSV拡張子チェック
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public static bool IsCsvExtension(string fileName)
        {
            try
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 取込ファイルパス作成
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>取込ファイルパス</returns>
        public static string CreateImportFilePath(string categoryName)
        {
            try
            {
                // ファイル名(日付_ファイル名)
                var tmpFileName = string.Concat(
                    categoryName, "_", DateTime.Now.ToString("yyyyMMddHHmmssfff"), ".csv"
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// CSVファイル読み取り
        /// </summary>
        /// <param name="csvFilePath">CSVファイルのパス</param>
        /// <param name="headerCount">ヘッダー数</param>
        /// <returns>配列</returns>
        public static List<string[]> ReadCsvFile(string csvFilePath, int headerCount)
        {
            try
            {
                List<string[]> csvLines = new();

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (StreamReader reader = new StreamReader(csvFilePath, Encoding.GetEncoding("Shift_JIS")))
                {
                    while (!reader.EndOfStream)
                    {
                        // CSVファイルの一行をカンマ毎に分けて配列に格納
                        string line = reader.ReadLine();
                        string[] lineArr = Regex.Split(line, @"(?<=,)(?=(?:[^""]*""[^""]*"")*[^""]*$)");
                        for (int i = 0; i < lineArr.Count(); i++)
                        {
                            if (lineArr[i] != null)
                            {
                                lineArr[i] = lineArr[i].Trim(',').Trim('"').ToString();
                            }
                        }
                        csvLines.Add(lineArr.Take(headerCount).ToArray());
                    }
                    reader.Close();
                }
                return csvLines;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// CSVファイルコピー
        /// </summary>
        /// <param name="readFile"></param>
        /// <param name="gamenName"></param>
        /// <returns>ファイルパス</returns>
        public async static Task<string> CopyCsvFile(IFormFile readFile, string gamenName)
        {
            try
            {
                // 取込ファイルパス作成
                string filePath = CreateImportFilePath(gamenName);

                // ファイルコピー
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await readFile.CopyToAsync(stream);
                }

                return filePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// CSVファイルデータチェック
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="headerSettings"></param>
        /// <returns></returns>
        public static bool CheckCsvData(List<string[]> lines, Dictionary<int, string> headerSettings)
        {
            try
            {
                // 配列がnullの場合はエラー
                if (lines == null || lines.Count == 0)
                    return false;

                // ヘッダーがnullの場合はエラー
                var header = lines.First();
                if (header == null || header.Length == 0)
                    return false;

                // ヘッダー内にnullがある場合はエラー
                foreach (var column in header)
                {
                    if (string.IsNullOrWhiteSpace(column))
                        return false;
                }

                // 空行削除
                lines.RemoveAll(line => string.IsNullOrWhiteSpace(string.Join("", line)));

                // 行が1つだけ(ヘッダーのみ)の場合はエラー
                if (lines.Count == 1)
                    return false;

                // ヘッダー名が正しくない場合はエラー
                bool isValidHeader = CheckIsValidHeader(lines[0], headerSettings);
                if (!isValidHeader)
                {
                    return false;
                }

                // ヘッダーの列数を取得
                int columnCount = header.Length;

                // データ行の列数チェック
                foreach (var line in lines.Skip(1))
                {
                    // 行がnullの場合は次の行に進む
                    if (line == null || line.Length == 0)
                        continue;

                    // 列数がヘッダーの列数と一致しない場合はエラー
                    if (line.Length != columnCount)
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ヘッダー名チェック
        /// </summary>
        /// <param name="headerLines"></param>
        /// <param name="headerSettings"></param>
        /// <returns></returns>
        public static bool CheckIsValidHeader(string[] headerLines, Dictionary<int, string> headerSettings)
        {
            try
            {
                // ヘッダー名が一致しない場合はエラー
                foreach (var setItem in headerSettings)
                {
                    if (setItem.Key >= headerLines.Length || !setItem.Value.Equals(headerLines[setItem.Key]))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw; 
            }
        }

        /// <summary>
        /// ファイルを削除
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (!string.Empty.Equals(filePath) || File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
