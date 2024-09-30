﻿using OfficeOpenXml;
using System.Data;

namespace ai_truck_load_measurement.Commons
{
    public static class CreateFileController
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
        public static (bool, string) CheckCreateExcel(DataTable dtOne, DataTable dtTwo,  string tmpFilename, string folderName, string headerName, bool sheetTwo, bool sheetAboutImport, List<string> aboutImport, string sheetNameOne, string sheetNameTwo)
        {
            try
            {
                // 出力フォルダパス
                var in_out = "export";

#if DEBUG
                // デバッグ
                // ...\tec-shipping-management-web\wwwroot\sv-esm-bk\export\
                var section = "developmentFolderPath";
                var rootPath = Directory.GetCurrentDirectory();
                string _folderPath;

                try
                {
                    var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false);
                    var configuration = builder.Build();
                    _folderPath = configuration.GetSection(section).GetValue<string>(in_out + ":" + folderName);

                    if (folderName == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    throw;
                }

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
                List<string> headerList = CreateHeaderList(headerName);

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
            catch(Exception)
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
                    if (ws== null)
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
        public static List<string> CreateHeaderList(string headerName)
        {
            List<string> headerList = new();

            switch (headerName)
            {
                case "MUsers":
                    headerList.Add("ID");
                    headerList.Add("ログインID");
                    headerList.Add("会社名");
                    headerList.Add("ユーザー名");
                    headerList.Add("パスワード");
                    headerList.Add("管理権限区分");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    headerList.Add("削除フラグ");
                    break;
                case "MShippingLanes":
                    headerList.Add("ID");
                    headerList.Add("出荷レーン名");
                    headerList.Add("出荷レーン連番");
                    headerList.Add("トラックヤード名");
                    headerList.Add("工場区分");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    headerList.Add("削除フラグ");
                    break;
                case "ShippingPlanImport":
                    headerList.Add("送信先コード");
                    headerList.Add("物理拠点");
                    headerList.Add("物理工区");
                    headerList.Add("物理出荷場");
                    headerList.Add("物理拠点名");
                    headerList.Add("事業体");
                    headerList.Add("仕入先");
                    headerList.Add("仕入先工区");
                    headerList.Add("仕入先出荷場");
                    headerList.Add("納入先");
                    headerList.Add("納入先工区");
                    headerList.Add("受入");
                    headerList.Add("納入先名称");
                    headerList.Add("オーダー日付");
                    headerList.Add("オーダーSeq");
                    headerList.Add("かんばん受信日時");
                    headerList.Add("仕入先発ルート");
                    headerList.Add("仕入先発便＃");
                    headerList.Add("仕入先発ルート日付");
                    headerList.Add("仕入先発日時");
                    break;

                case "ShippingPlanInquiryByProductNumber":
                    headerList.Add("受入");
                    headerList.Add("納入先名称");
                    headerList.Add("背番号");
                    headerList.Add("品番");
                    headerList.Add("オーダー日付");
                    headerList.Add("オーダーSeq");
                    headerList.Add("仕入先発ルート日付");
                    headerList.Add("仕入先発便＃");
                    headerList.Add("箱数");
                    headerList.Add("総数");
                    headerList.Add("仕入先出荷場");
                    headerList.Add("仕入先");
                    headerList.Add("仕入先発日時");
                    headerList.Add("出荷レーン状況名");
                    headerList.Add("送信先コード");
                    headerList.Add("物理拠点");
                    headerList.Add("物理工区");
                    headerList.Add("物理出荷場");
                    headerList.Add("物理拠点名");
                    headerList.Add("事業体");
                    headerList.Add("仕入先工区");
                    headerList.Add("納入先");
                    headerList.Add("納入先工区");
                    headerList.Add("かんばん受信日時");
                    headerList.Add("仕入先発ルート");
                    headerList.Add("出荷計画完全一致");
                    headerList.Add("作成日");
                    headerList.Add("作成者");
                    headerList.Add("更新日");
                    headerList.Add("更新者");
                    break;

                case "ShippingPlanExport":

                    headerList.Add("送信先コード");
                    headerList.Add("物理拠点");
                    headerList.Add("物理工区");
                    headerList.Add("物理出荷場");
                    headerList.Add("物理拠点名");
                    headerList.Add("事業体");
                    headerList.Add("仕入先");
                    headerList.Add("仕入先工区");
                    headerList.Add("仕入先出荷場");
                    headerList.Add("納入先");
                    headerList.Add("納入先工区");
                    headerList.Add("受入");
                    headerList.Add("納入先名称");
                    headerList.Add("オーダー日付");
                    headerList.Add("オーダーSeq");
                    headerList.Add("かんばん受信日時");
                    headerList.Add("仕入先発ルート");
                    headerList.Add("仕入先発便＃");
                    headerList.Add("仕入先発ルート日付");
                    headerList.Add("仕入先発日時");
                    headerList.Add("作成日時");
                    headerList.Add("作成者");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    break;
                case "MRoutes":
                    headerList.Add("ID");
                    headerList.Add("適用日");
                    headerList.Add("工場区分");
                    headerList.Add("物流区分");
                    headerList.Add("ルートコード");
                    headerList.Add("便名称");
                    headerList.Add("車目／便");
                    headerList.Add("出荷開始予定時刻");
                    headerList.Add("出荷終了予定時刻");
                    headerList.Add("積込開始予定時刻");
                    headerList.Add("積込終了予定時刻");
                    headerList.Add("出荷レーン1");
                    headerList.Add("出荷レーン2");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    headerList.Add("削除フラグ");
                    break;
                case "ShippingRecord":
                    headerList.Add("ID");
                    headerList.Add("工場区分");
                    headerList.Add("ルートコード");
                    headerList.Add("便名称");
                    headerList.Add("車目／便");
                    headerList.Add("出荷レーン1");
                    headerList.Add("出荷レーン2");
                    headerList.Add("積込日");
                    headerList.Add("出荷開始日時");
                    headerList.Add("出荷終了日時");
                    headerList.Add("積込開始日時");
                    headerList.Add("積込終了日時");
                    headerList.Add("積込終了ユーザー名");
                    headerList.Add("合計出荷パレット数");
                    headerList.Add("合計出荷ポリ箱数");
                    headerList.Add("積込パレット数");
                    headerList.Add("積込ポリ箱数");
                    headerList.Add("作成日時");
                    headerList.Add("更新日時");
                    headerList.Add("出荷指示書総枚数");
                    break;
                case "PalletRecord":
                    headerList.Add("ID");
                    headerList.Add("工場区分");
                    headerList.Add("ルートコード");
                    headerList.Add("便名称");
                    headerList.Add("車目／便");
                    headerList.Add("積込日");
                    headerList.Add("出荷鉄パレット数");
                    headerList.Add("出荷ポリ箱数");
                    headerList.Add("積込鉄パレット数");
                    headerList.Add("積込ポリ箱数");
                    headerList.Add("確定鉄パレット数");
                    headerList.Add("確定ポリ箱数");
                    headerList.Add("確定ユーザー名");
                    headerList.Add("作成日時");
                    headerList.Add("作成者");
                    break;
                case "MCustomers":
                    headerList.Add("ID");
                    headerList.Add("得意先コード");
                    headerList.Add("得意先名");
                    headerList.Add("納入先");
                    headerList.Add("受入");
                    headerList.Add("仕入先");
                    headerList.Add("仕入先工区");
                    headerList.Add("出荷場");
                    headerList.Add("更新日時");
                    headerList.Add("更新者");
                    headerList.Add("削除フラグ");
                    break;
            }
            return headerList;
        }
    }
}