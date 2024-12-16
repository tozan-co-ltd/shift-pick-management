using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace ai_truck_load_measurement.Commons
{
    public static class CreateFile
    {
        /// <summary>
        /// DataTableをCSV形式の文字列に変換
        /// </summary>
        /// <param name="dataTable">データテーブル</param>
        /// <param name="filePath">ファイルパス</param>
        public static void ConvertDataTableToCsv(this System.Data.DataTable dataTable, string filePath)
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
                {
                    Directory.CreateDirectory(folderPath);
                }
                else
                {
                    // 削除するファイルの数を決定
                    CleanFolder(folderPath, 4);
                }

                // 取込ファイルパス
                return Path.Combine(folderPath, tmpFileName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ファイル名作成
        /// </summary>
        /// <param name="searchModel">検索モデル</param>
        /// <param name="gamenName">画面名</param>
        /// <returns></returns>
        public static string CreateFileName(string gamenName)
        {
            string fileName = string.Empty;
            try
            {
                // Trim object
                gamenName = gamenName == null ? string.Empty : gamenName.Trim();

                return string.Concat(fileName, gamenName, ".xlsx");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 削除するファイルの数を決定
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <param name="filesToKeep">保持するファイルの数</param>
        static void CleanFolder(string folderPath, int filesToKeep)
        {
            // ディレクトリが存在するかどうかを確認
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);

                // ファイルリストを作成日順に並べ替える
                Array.Sort(files, (x, y) => new FileInfo(x).CreationTime.CompareTo(new FileInfo(y).CreationTime));

                // 削除するファイルの数を決定
                int filesToDelete = files.Length - filesToKeep;

                // 最も古いファイルを削除
                for (int i = 0; i < filesToDelete; i++)
                {
                    File.Delete(files[i]);
                }
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

        /// <summary>
        /// Excel作成チェック
        /// </summary>
        /// <param name="dtOne">DataTable</param>
        /// <param name="dtTwo">DataTable</param>
        /// <param name="tmpFilename">tmpファイル名</param>
        /// <param name="folderName">フォルダ名</param>
        /// <param name="headerName">ヘッダー名</param>
        /// <returns>作成結果,出力フォルダフルパス</returns>
        public static (bool, string) CheckCreateExcel(DataTable dtOne, DataTable dtTwo, string tmpFilename, bool sheetTwo, string sheetNameOne, string sheetNameTwo, string gamenName)
        {
            try
            {
                // シート名
                var sheetName = tmpFilename.Replace(".xlsx", "");

                // ヘッダーリストを作成
                List<string> headerList = CreateHeaderList(gamenName);
                List<string> headerListTwo = new();
                if (sheetTwo)
                {
                    headerListTwo = CreateHeaderListTwo(gamenName);
                }

                // Excelファイル作成
                bool createRs;
                if (sheetTwo == true)
                    createRs = CreateTwoSheetExcel(dtOne, dtTwo, tmpFilename, headerList, headerListTwo, sheetNameOne, sheetNameTwo);
                else
                    createRs = CreateExcel(dtOne, tmpFilename, headerList, headerListTwo, sheetName);

                return (createRs, tmpFilename);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Excel作成
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="exportfileFullPath"></param>
        /// <param name="headerList"></param>
        /// <param name="sheetName"></param>
        /// <returns>作成結果</returns>
        public static bool CreateExcel(DataTable dt, string exportfileFullPath, List<string> headerList, List<string> headerList2, string sheetName)
        {
            // データがない場合はヘッダーのみ作成
            if (dt == null || dt.Rows.Count == 0)
            {
                // ファイル情報取得
                FileInfo fileInfo = new(exportfileFullPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName);
                    // シート1が存在場合シート1追加
                    if (ws == null)
                        package.Workbook.Worksheets.Add(sheetName);

                    using ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName];
                    // フィルター設定
                    sheet.Cells["A1:AM1"].AutoFilter = true;
                    // ウィンドウ枠の固定
                    sheet.View.FreezePanes(2, 1);
                    // セル自動選択
                    sheet.Select("A2");

                    // タイトル行が指定されているときは、タイトル行をセットする
                    if (headerList != null && headerList.Count > 0)
                    {
                        for (int i = 0; i < headerList.Count; i++)
                        {
                            sheet.Cells[1, i + 1].Value = headerList[i];
                        }
                    }
                    // 保管
                    package.Save();
                }
                return true;
            }

            // 出力ファイルパスが未指定の場合は中断する
            if (String.IsNullOrWhiteSpace(exportfileFullPath))
            {
                return false;
            }

            // 既にファイルが存在している場合は削除する
            if (File.Exists(exportfileFullPath))
            {
                File.Delete(exportfileFullPath);
            }

            try
            {
                var startIndex = 1;
                var printHeader = true;

                // ファイル情報取得
                FileInfo fileInfo = new(exportfileFullPath);
                // ワークシート作成
                using var package = new ExcelPackage(fileInfo);
                // シート追加
                package.Workbook.Worksheets.Add(sheetName);
                // シート取得
                using ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName];
                // フィルター設定
                sheet.Cells["A1:AM1"].AutoFilter = true;
                // ウィンドウ枠の固定
                sheet.View.FreezePanes(2, 1);
                // セル自動選択
                sheet.Select("A2");

                // タイトル行が指定されているときは、タイトル行をセットする
                if (headerList != null && headerList.Count > 0)
                {
                    for (int i = 0; i < headerList.Count; i++)
                    {
                        sheet.Cells[1, i + 1].Value = headerList[i];
                    }
                    // 開始行番号をセット
                    startIndex = 2;
                    // タイトル出力済なので、列名は出力しない
                    printHeader = false;
                }

                // データセット
                sheet.Cells[startIndex, 1].LoadFromDataTable(dt, printHeader);

                // 保管
                package.Save();
                return true;
            }
            catch (Exception)
            {
                // 失敗した場合は出力用ファイル削除
                if (File.Exists(exportfileFullPath))
                {
                    File.Delete(exportfileFullPath);
                }
                throw;
            }
        }


        /// <summary>
        /// 2シートExcel作成
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="exportfileFullPath"></param>
        /// <param name="headerList"></param>
        /// <param name="sheetName"></param>
        /// <param name="sheetName2"></param>
        /// <param name="aboutImport"></param>
        /// <returns>作成結果</returns>
        public static bool CreateTwoSheetExcel(DataTable dt, DataTable dtTwo, string exportfileFullPath, List<string> headerList, List<string> headerListTwo, string sheetName, string sheetName2)
        {
            // データがない場合はヘッダーのみ作成
            if (dt == null || dt.Rows.Count == 0)
            {
                // ファイル情報取得
                FileInfo fileInfo = new(exportfileFullPath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var wsOne = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName);
                    // シート1が存在場合シート1追加
                    if (wsOne == null)
                        package.Workbook.Worksheets.Add(sheetName);

                    using ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName];
                    // フィルター設定
                    sheet.Cells["A1:AM1"].AutoFilter = true;
                    // ウィンドウ枠の固定
                    sheet.View.FreezePanes(2, 1);
                    // セル自動選択
                    sheet.Select("A2");

                    // タイトル行が指定されているときは、タイトル行をセットする
                    if (headerList != null && headerList.Count > 0)
                    {
                        for (int i = 0; i < headerList.Count; i++)
                        {
                            sheet.Cells[1, i + 1].Value = headerList[i];
                        }
                    }

                    // シート2が存在場合シート2追加
                    var wsTwo = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName2);
                    if (wsOne == null)
                        package.Workbook.Worksheets.Add(sheetName2);

                    using ExcelWorksheet sheetTwo = package.Workbook.Worksheets[sheetName2];

                    // タイトル行が指定されているときは、タイトル行をセットする
                    if (headerListTwo != null && headerListTwo.Count > 0)
                    {
                        for (int i = 0; i < headerListTwo.Count; i++)
                        {
                            sheetTwo.Cells[1, i + 1].Value = headerListTwo[i];
                        }
                    }

                    // ファイル保存
                    package.Save();
                }
                return true;
            }

            // 出力ファイルパスが未指定の場合は中断する
            if (String.IsNullOrWhiteSpace(exportfileFullPath))
            {
                return false;
            }

            // 既にファイルが存在している場合は削除する
            if (File.Exists(exportfileFullPath))
            {
                File.Delete(exportfileFullPath);
            }

            try
            {
                var startIndex = 1;
                var printHeader = true;

                // ファイル情報取得
                FileInfo fileInfo = new(exportfileFullPath);
                // ワークシート作成
                using var package = new ExcelPackage(fileInfo);
                // シート追加
                package.Workbook.Worksheets.Add(sheetName);
                // シート取得
                using ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName];
                // フィルター設定
                //sheet.Cells["A1:AM1"].AutoFilter = true;
                // ウィンドウ枠の固定
                sheet.View.FreezePanes(2, 1);
                // セル自動選択
                sheet.Select("A2");

                // タイトル行が指定されているときは、タイトル行をセットする
                if (headerList != null && headerList.Count > 0)
                {
                    for (int i = 0; i < headerList.Count; i++)
                    {
                        sheet.Cells[1, i + 1].Value = headerList[i];
                    }
                    // 開始行番号をセット
                    startIndex = 2;
                    // タイトル出力済なので、列名は出力しない
                    printHeader = false;
                }

                // シート2 追加
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName2);
                // シート2取得
                using ExcelWorksheet sheetTwo = package.Workbook.Worksheets[sheetName2];
                // フィルター設定
                sheetTwo.Cells["A1:AM1"].AutoFilter = true;

                // タイトル行が指定されているときは、タイトル行をセットする
                if (headerListTwo != null && headerListTwo.Count > 0)
                {
                    for (int i = 0; i < headerListTwo.Count; i++)
                    {
                        sheetTwo.Cells[1, i + 1].Value = headerListTwo[i];
                    }
                    // 開始行番号をセット
                    startIndex = 2;
                    // タイトル出力済なので、列名は出力しない
                    printHeader = false;
                }

                // シート1 データセット
                sheet.Cells[startIndex, 1].LoadFromDataTable(dt, printHeader);
                // シート2 データセット
                sheetTwo.Cells[startIndex, 1].LoadFromDataTable(dtTwo, printHeader);

                // ファイル保存
                package.Save();
                return true;
            }
            catch (Exception)
            {
                // 失敗した場合は出力用ファイル削除
                if (File.Exists(exportfileFullPath))
                {
                    File.Delete(exportfileFullPath);
                }
                throw;
            }
        }

        /// <summary>
        /// ヘッダーリスト作成
        /// </summary>
        /// <param name="headerName">ヘッダー名</param>
        /// <returns>ヘッダーリスト</returns>
        public static List<string> CreateHeaderList(string gamenName)
        {
            List<string> headerList = new();
            switch (gamenName)
            {
                case "車両マスター":
                    headerList.Add("車両ID");
                    headerList.Add("車両番号");
                    headerList.Add("識別番号");
                    headerList.Add("削除フラグ");
                    headerList.Add("作成日時");
                    headerList.Add("作成者");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    break;
                case "便マスター":
                    headerList.Add("便ID");
                    headerList.Add("便名称");
                    headerList.Add("乗務員");
                    headerList.Add("車両番号");
                    headerList.Add("識別番号");
                    headerList.Add("昼勤開始時間");
                    headerList.Add("適用開始日時");
                    headerList.Add("適用終了日時");
                    headerList.Add("作成日時");
                    headerList.Add("作成者");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    break;
                case "実績出力":
                    headerList.Add("項目名");
                    headerList.Add("検索条件");
                    break;
                case "荷量推移":
                    headerList.Add("項目名");
                    headerList.Add("検索条件");
                    break;
            }
            return headerList;
        }

        /// <summary>
        /// シート2枚目のヘッダーリスト作成
        /// </summary>
        /// <param name="headerName">ヘッダー名</param>
        /// <returns>ヘッダーリスト</returns>
        public static List<string> CreateHeaderListTwo(string gamenName)
        {
            List<string> headerList = new();
            switch (gamenName)
            {
                case "便マスター":
                    headerList.Add("便ID");
                    headerList.Add("便名称");
                    headerList.Add("乗務員");
                    headerList.Add("車両番号");
                    headerList.Add("識別番号");
                    headerList.Add("昼勤開始時間");
                    headerList.Add("便マスター適用開始日時");
                    headerList.Add("便マスター適用終了日時");
                    headerList.Add("便マスター作成日時");
                    headerList.Add("便マスター作成者");
                    headerList.Add("便マスター更新日時");
                    headerList.Add("便マスター更新者");
                    headerList.Add("便枝番ID");
                    headerList.Add("枝連番");
                    headerList.Add("到着予定時間");
                    headerList.Add("出発予定時間");
                    headerList.Add("便枝番マスター適用開始日時");
                    headerList.Add("便枝番マスター適用終了日時");
                    headerList.Add("便枝番マスター作成日時");
                    headerList.Add("便枝番マスター作成者");
                    headerList.Add("便枝番マスター更新日時");
                    headerList.Add("便枝番マスター更新者");
                    break;
                default:
                    headerList.Add("便名称");
                    headerList.Add("便枝番");
                    headerList.Add("乗務員");
                    headerList.Add("ステーションID");
                    headerList.Add("車両番号");
                    headerList.Add("識別番号");
                    headerList.Add("到着予定時間");
                    headerList.Add("出発予定時間");
                    headerList.Add("稼働日");
                    headerList.Add("到着日時");
                    headerList.Add("出発日時");
                    headerList.Add("到着荷量(%)");
                    headerList.Add("出発荷量(%)");
                    headerList.Add("到着荷量画像パス");
                    headerList.Add("出発荷量画像パス");
                    break;
            }
            return headerList;
        }
    }
}

