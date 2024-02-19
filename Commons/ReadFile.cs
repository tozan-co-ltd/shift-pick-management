using mar_sumaken_web.Models;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace mar_sumaken_web.Commons
{
    public static class ReadFile
    {
        /// <summary>
        /// CSVファイルデータ読取
        /// </summary>
        /// <param name="csvModel">入力モデル</param>
        /// <param name="gamenName">画面名</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        public async static Task<(string, List<string[]>?)> ReadCsv(CsvFileInputModel csvModel, string gamenName, int userId)
        {
            var errorMsg = string.Empty;
            var lines = new List<string[]>();

            try
            {
                // ファイル形式チェック
                if (!IsCsvFile(csvModel.FileName))
                {
                    // エラーメッセージ取得
                    return ("ファイルの形式が正しくありません。", null);
                };

                // ファイルコピー
                // 取込ファイルパス
                string importFilePath = await CopyFile(csvModel.ImportFile, csvModel.FileName, gamenName, userId);

                // CSVファイルデータ読み取り
                lines = ReadCsvFile(importFilePath, csvModel.HeaderColumnCount);

                // CSVファイルデータチェック
                bool isValidCsv = CheckCsvData(lines, csvModel.HeaderSettings);
                if (!isValidCsv)
                {
                    // エラーメッセージ取得
                    return ("ファイルの内容が正しくありません。", null);
                }

                return (errorMsg, lines);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// CSVフィオルを作る
        /// </summary>
        /// <param name="dataTable">データテーブル</param>
        /// <param name="filePath">ファイルパス</param>
        public static void ToCSV(this DataTable dataTable, string filePath)
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
        /// CSV拡張子チェック
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public static bool IsCsvFile(string fileName)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// CSVファイル保存 
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="categoryName">保存フォルダー</param>
        /// <returns>取込ファイルパス</returns>
        public static string CreateImportFilePath(string fileName, int userId, string categoryName)
        {
            try
            {
                // ファイル名(日付_ファイル名)
                var tmpFileName = string.Concat(
                    DateTime.Now.ToString("yyyyMMddHHmmssfff"), "_", userId, "_", Path.GetFileName(fileName)
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
            try
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
                        string[] lineArr = Regex.Split(line, @"(?<=,)(?=(?:[^""]*""[^""]*"")*[^""]*$)");

                        for (int i = 0; i < lineArr.Count(); i++)
                        {
                            if (lineArr[i] != null)
                            {
                                lineArr[i] = lineArr[i].Trim(',').Trim('"').ToString();
                            }
                        }
                        //strに格納
                        csvLines.Add(lineArr.Take(itemCount).ToArray());
                    }

                    //CSVファイルを閉じる
                    reader.Close();
                }
                return csvLines;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ファイルコピ
        /// </summary>
        /// <param name="readFile"></param>
        /// <param name="fileName"></param>
        /// <param name="userId"></param>
        /// <param name="gamenName"></param>
        /// <returns>ファイルパス</returns>
        public async static Task<string> CopyFile(IFormFile readFile, string fileName, string gamenName, int userId)
        {
            try
            {
                // 取込ファイルパス
                string filePath = CreateImportFilePath(fileName, userId, gamenName);

                // ファイルコピー
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await readFile.CopyToAsync(stream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// CSVファイルデータチェック
        /// </summary>
        /// <param name="lines">CSV行配列</param>
        public static bool CheckCsvData(List<string[]> lines, Dictionary<int, string> headerSettings)
        {
            try
            {
                // リストが null または空の場合、エラー
                if (lines == null || lines.Count == 0)
                    return false;

                // 最初の行（ヘッダー）を確認する
                var header = lines.First();
                if (header == null || header.Length == 0)
                    return false;

                // ヘッダー内の各要素をチェックする
                foreach (var column in header)
                {
                    if (string.IsNullOrWhiteSpace(column))
                        return false; // ヘッダーに null または空の要素がある場合、エラー
                }

                // 行が1つだけ（ヘッダーのみ）の場合、エラー
                if (lines.Count == 1)
                    return false;

                // ヘッダー名チェック
                bool isValidHeader = CheckIsValidHeader(lines[0], headerSettings);
                if (!isValidHeader)
                {
                    return false;
                }

                // ヘッダーの列数を取得する
                int columnCount = header.Length;

                // 空行削除
                lines.RemoveAll(line => string.IsNullOrWhiteSpace(string.Join("", line)));

                // 各データ行の列数をチェックする
                foreach (var line in lines.Skip(1)) // 最初の行（ヘッダー）をスキップして2行目からチェックする
                {
                    // 行が null もしくは空の場合、次の行に進む
                    if (line == null || line.Length == 0)
                        continue;

                    // データ行の列数をチェックする
                    if (line.Length != columnCount)
                        return false; // 列数がヘッダーと一致しない場合、エラー
                }

                // エラーがない場合、データは有効
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ヘッダー名チェック
        /// </summary>
        /// <param name="headerCheck">ヘッダー名リスト</param>
        /// <param name="headerSettings">モデルヘッダ名リスト</param>
        /// <returns></returns>
        public static bool CheckIsValidHeader(string[] headerCheck, Dictionary<int, string> headerSettings)
        {
            try
            {
                foreach (var setItem in headerSettings)
                {
                    if (setItem.Key >= headerCheck.Length || !setItem.Value.Equals(headerCheck[setItem.Key]))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }

    }
}
