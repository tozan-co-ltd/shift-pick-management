using ai_truck_load_measurement.Models;
using OfficeOpenXml;
using System.Data;

namespace ai_truck_load_measurement.Commons
{
    public class ExcelController 
    {

        /// <summary>
        /// Excel作成チェック
        /// </summary>
        /// <param name="dtOne">DataTable</param>
        /// <param name="dtTwo">DataTable</param>
        /// <param name="tmpFilename">tmpファイル名</param>
        /// <param name="folderName">フォルダ名</param>
        /// <param name="headerName">ヘッダー名</param>
        /// <returns>作成結果,出力フォルダフルパス</returns>
        public static (bool, string) CheckCreateExcel(DataTable dtOne, DataTable dtTwo, string tmpFilename, string folderName, string headerName, bool sheetTwo, bool sheetAboutImport, List<string> aboutImport, string sheetNameOne, string sheetNameTwo)
        {
            try
            {
#if DEBUG
                // デバッグ
                // ...\tec-shipping-management-web\wwwroot\sv-esm-bk\export\
                var rootPath = Directory.GetCurrentDirectory();
                string _folderPath = "\\wwwroot\\sv-esm-bk\\export\\";

                var folderPath = Path.Combine(rootPath, _folderPath);
#else
                // 本番環境
                // \\\\sv-esm-bk\share\export\
                var section = "productionFolderPath";
                var folderPath = ConnectToBackupNas.GetBackupNasConnectionString(section, in_out, folderName);
#endif

                // フォルダが存在しない場合は新規作成
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 出力フォルダフルパス
                var expPath = Path.Combine(folderPath, tmpFilename);
                // シート名
                var sheetName = tmpFilename.Replace(".xlsx", ""); ;
                var sheetAbout = "取込について";

                // ヘッダーリストを作成
                List<string> headerList = CreateHeaderList();

                // Excelファイル作成
                bool createRs;
                if (sheetAboutImport == true && sheetTwo == false)
                    createRs = CreateTwoSheetExcel(dtOne, expPath, headerList, sheetName, sheetAbout, aboutImport);
                else if (sheetAboutImport == true && sheetTwo == true)
                    createRs = CreateThreeSheetExcel(dtOne, dtTwo, expPath, headerList, sheetNameOne, sheetNameTwo, sheetAbout, aboutImport);
                else
                    createRs = CreateExcel(dtOne, expPath, headerList, sheetName);

                return (createRs, expPath);
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
        public static bool CreateExcel(DataTable dt, string exportfileFullPath, List<string> headerList, string sheetName)
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
            // 出力フォルダが存在しない場合は中断する
            if (!Directory.Exists(Path.GetDirectoryName(exportfileFullPath)))
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
        public static bool CreateTwoSheetExcel(DataTable dt, string exportfileFullPath, List<string> headerList, string sheetName, string sheetName2, List<string> aboutImport)
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

                    using ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetName2];

                    // シート2内容をセットする
                    if (aboutImport != null && aboutImport.Count > 0)
                    {
                        for (int i = 0; i < aboutImport.Count; i++)
                            worksheet.Cells[i + 1, 2].Value = aboutImport[i];
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
            // 出力フォルダが存在しない場合は中断する
            if (!Directory.Exists(Path.GetDirectoryName(exportfileFullPath)))
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

                // シート2 追加
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName2);

                // シート2内容をセットする
                if (aboutImport != null && aboutImport.Count > 0)
                {
                    for (int i = 0; i < aboutImport.Count; i++)
                        worksheet.Cells[i + 1, 2].Value = aboutImport[i];
                }

                // データセット
                sheet.Cells[startIndex, 1].LoadFromDataTable(dt, printHeader);

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
        /// 2シートExcel作成
        /// </summary>
        /// <param name="dtOne"></param>
        /// <param name="dtTwo"></param>
        /// <param name="exportfileFullPath"></param>
        /// <param name="headerList"></param>
        /// <param name="sheetName"></param>
        /// <param name="sheetName2"></param>
        /// <param name="aboutImport"></param>
        /// <returns>作成結果</returns>
        public static bool CreateThreeSheetExcel(DataTable dtOne, DataTable dtTwo, string exportfileFullPath, List<string> headerList, string sheetName, string sheetName2, string sheetAbout, List<string> aboutImport)
        {
            // データがない場合はヘッダーのみ作成
            if (dtOne == null || dtOne.Rows.Count == 0)
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

                    var wsTwo = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName2);
                    // シート2が存在場合シート2追加
                    if (wsOne == null)
                        package.Workbook.Worksheets.Add(sheetName2);

                    using ExcelWorksheet sheetTwo = package.Workbook.Worksheets[sheetName2];

                    // タイトル行が指定されているときは、タイトル行をセットする
                    if (headerList != null && headerList.Count > 0)
                    {
                        for (int i = 0; i < headerList.Count; i++)
                        {
                            sheetTwo.Cells[1, i + 1].Value = headerList[i];
                        }
                    }

                    //取込についてシートが存在場合シート2追加
                    var wsAbout = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetAbout);
                    if (wsOne == null)
                        package.Workbook.Worksheets.Add(sheetAbout);

                    using ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetAbout];

                    // 取込についてシート内容をセットする
                    if (aboutImport != null && aboutImport.Count > 0)
                    {
                        for (int i = 0; i < aboutImport.Count; i++)
                            worksheet.Cells[i + 1, 2].Value = aboutImport[i];
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
            // 出力フォルダが存在しない場合は中断する
            if (!Directory.Exists(Path.GetDirectoryName(exportfileFullPath)))
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

                // シート2追加
                package.Workbook.Worksheets.Add(sheetName2);
                // シート2取得
                using ExcelWorksheet sheetTwo = package.Workbook.Worksheets[sheetName2];
                // フィルター設定
                sheetTwo.Cells["A1:AM1"].AutoFilter = true;

                // タイトル行が指定されているときは、タイトル行をセットする
                if (headerList != null && headerList.Count > 0)
                {
                    for (int i = 0; i < headerList.Count; i++)
                    {
                        sheetTwo.Cells[1, i + 1].Value = headerList[i];
                    }
                    // 開始行番号をセット
                    startIndex = 2;
                    // タイトル出力済なので、列名は出力しない
                    printHeader = false;
                }

                // 取込についてシート 追加
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetAbout);

                // 取込についてシート内容をセットする
                if (aboutImport != null && aboutImport.Count > 0)
                {
                    for (int i = 0; i < aboutImport.Count; i++)
                        worksheet.Cells[i + 1, 2].Value = aboutImport[i];
                }

                // シート1 データセット
                sheet.Cells[startIndex, 1].LoadFromDataTable(dtOne, printHeader);

                // シート12データセット2
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
        public static List<string> CreateHeaderList()
        {
            List<string> headerList = new();
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("TruckID"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("TruckNumber"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("IdentifyNumber"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("IsDeleted"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("CreatedAt"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("CreatedBy"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedAt"));
            headerList.Add(Utils.GetDisplayName<M_TruckModel>("UpdatedBy"));
            return headerList;
        }
    }
}

