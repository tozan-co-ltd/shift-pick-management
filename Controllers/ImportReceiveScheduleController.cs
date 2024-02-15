using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Transactions;

namespace mar_sumaken_web.Controllers
{
    public class ImportReceiveScheduleController : BaseController
    {
        private readonly ILogger<ImportReceiveScheduleController> _logger;  

        /// <summary>
        /// ヘッダー列数取得
        /// </summary>
        public readonly int Header_Column_Count = 5;

        public ImportReceiveScheduleController(ILogger<ImportReceiveScheduleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 入荷予定取込画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();

            // 管理権限区分が1(管理者)でない場合はエラーとする
            if (user == null || user.AuthorizedKubun != 1)
            {
                // エラーメッセージ取得
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }

            var model = new D_FileImportModel();

            // 入荷予定取込一覧取得
            var listD_FileImport = GetListD_FileImport(ClaimsLoginUserData().DatabaseName);
            model.D_FileImportList = listD_FileImport;

            return View(model);
        }

        /// <summary>
        /// 入荷予定取込一覧取得
        /// </summary>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<D_FileImportModel> GetListD_FileImport(string databaseName)
        {
            // SQL作成
            var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport("入荷予定取込");

            // DB接続
            List<D_FileImportModel> strList = D_FileImportConnectController.ConnectD_FileImport(sql, databaseName);

            return strList;
        }

        // <summary>
        /// Excel取込
        /// <param name="FileUpload">ファイル</param>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportExcel(List<IFormFile> FileUpload, int DepoID)
        {
            try
            {
                // log取得
                _logger.LogInformation($"Csv取込開始");

                var files = FileUpload;

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーメッセージ取得
                    return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                }


                // モデルリスト取得
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        List<D_ReceiveScheduleModel> importModelList = new();
                        List<string> errorList = new List<string>();
                        var fileName = file.FileName;
                        if (file.Length > 0)
                        {

                            // ファイル形式チェック
                            if (!Utils.IsCsvFile(fileName))
                            {
                                // エラーメッセージ取得
                                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                            };

                            // 取込ファイルパス
                            string importFilePath = Utils.CreateImportFilePath(fileName, user.UserID, "ReceiveSchedule");

                            // ファイルコピー
                            using (var stream = new FileStream(importFilePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // CSVファイルデータ読み取り
                            var lines = Utils.ReadCsvFile(importFilePath, Header_Column_Count);

                            // CSVファイルデータチェック
                            bool isValidCsv = Utils.CheckCsvData(lines);
                            if (!isValidCsv)
                            {
                                // エラーメッセージ取得
                                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                            }

                            // 空行削除
                            for (var i = lines.Count - 1; i >= 0; i--)
                            {
                                if (string.IsNullOrWhiteSpace(string.Join("", lines[i])))
                                    lines.RemoveAt(i);
                            }

                            // 読み取りデータを更新
                            int readCount = 0;
                            while (readCount < lines.Count)
                            {
                                D_ReceiveScheduleModel receiveSchedule = new()
                                {
                                    ImportFileName = fileName,
                                    SelectedDepoID = DepoID
                                };

                                // ヘッダー名チェック
                                if (readCount == 0)
                                {
                                    bool isValidHeader = CheckIsValidHeader(lines[0]);
                                    if (!isValidHeader)
                                    {
                                        // エラーメッセージ取得
                                        return NotFound(new { errorMessage = "正しいファイルを指定してください。" });
                                    }
                                }

                                // データチェック
                                if (readCount > 0)
                                {
                                    // 読み取りデータをモデルに設定
                                    receiveSchedule = SetReadDataInModel(receiveSchedule, lines, readCount);

                                    // 入荷予定データチェック
                                    var validationContext = new ValidationContext(receiveSchedule);
                                    var validationResults = new List<ValidationResult>();
                                    bool isValid = Validator.TryValidateObject(receiveSchedule, validationContext, validationResults, true);
                                    
                                    // エラーがあります。
                                    if (!isValid)
                                    {
                                        foreach(var err in validationResults)
                                        {
                                            var msg = readCount + "行目" + "　" + err.ErrorMessage;
                                            errorList.Add(msg);
                                        }
                                    }
                                    // リストに項目を追加
                                    importModelList.Add(receiveSchedule);
                                }
                                readCount++;
                            }
                        }
                        // エラーチェック
                        if (errorList.Count > 0)
                        {
                            var errorMsg = "<br/>" + string.Join("</br>", errorList);
                            return NotFound(new { errorMessage = errorMsg });
                        }

                        // 入荷予定データ書き込み
                        bool insertResult = D_ReceiveScheduleConnectController.InsertDReceiveSchedule(importModelList, DepoID, fileName, user);
                        if (!insertResult)
                        {
                            // エラーメッセージ取得 (E2011)
                            return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                        }
                    }
                }
                else
                {
                    // エラーメッセージ取得
                    // 「該当データがありません。」
                    return NotFound(new { errorMessage = "該当データがありません。" });
                }

                return Ok();
            }
            catch (SqlException ex)
            {
                return NotFound(new { errorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                return NotFound(new { errorMessage = exceptionMessage });
            }
        }

        /// <summary>
        /// ヘッダー名チェック
        /// </summary>
        /// <param name="headerCheck">チェックされたヘッダー</param>
        private static bool CheckIsValidHeader(string[] headerCheck)
        {
            Dictionary<int, string> headerSettings = new()
            {
                [0] = Utils.GetDisplayName<D_ReceiveScheduleModel>("CompanyCode"),
                [1] = Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleDate"),
                [2] = Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"),
                [3] = Utils.GetDisplayName<D_ReceiveScheduleModel>("LotNumber"),
                [4] = Utils.GetDisplayName<D_ReceiveScheduleModel>("Quantity")
            };

            foreach (var setItem in headerSettings)
            {
                if (!setItem.Value.Equals(headerCheck[setItem.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 読み取りデータをモデルに設定
        /// </summary>
        private static D_ReceiveScheduleModel SetReadDataInModel(D_ReceiveScheduleModel model, List<string[]> lines, int readCount)
        {
            model.CompanyCode = lines[readCount][0];
            model.ReceiveScheduleDate = lines[readCount][1];
            model.SupplierProductNumber = lines[readCount][2];
            model.LotNumber = lines[readCount][3];
            model.Quantity = lines[readCount][4];

            return model;
        }


        /// <summary>
        /// ファイル出力
        /// </summary>
        public JsonResult ExportFile()
        {
            string? errorMessage;
            try
            {
                // log取得
                _logger.LogInformation($"Excel出力開始");

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーコード：E2011
                    throw new Exception();
                }

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // ファイル名
                var tmpFilename = "入荷予定.csv";
                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換する
                Utils.ToCSV(dataTable, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception)
            {
                return Json(new { res = "NG", error = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// 入荷予定テーブルを作る
        /// </summary>
        private static DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("CompanyCode"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleDate"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("LotNumber"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<D_ReceiveScheduleModel>("Quantity"), typeof(string));

            return table;
        }
    }
}
