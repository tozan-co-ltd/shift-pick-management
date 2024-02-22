using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

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
            var model = new D_FileImportModel();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    // エラーメッセージ取得
                    return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
                }

                // SQL作成
                var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport("入荷予定取込");

                // DB接続
                List<D_FileImportModel> listD_FileImport = D_FileImportConnectController.ConnectD_FileImport(sql, user.DatabaseName);

                model.D_FileImportList = listD_FileImport;

                return View(model);
            }
            catch (Exception)
            {
                return View(model);
            }
        }

        // <summary>
        /// Csv取込
        /// <param name="FileUpload">ファイル</param>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportCsv(List<IFormFile> FileUpload, int DepoID, string GamenName)
        {
            string tempFilePath = string.Empty;
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
                    return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
                }

                // モデルリスト取得
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        List<D_ReceiveScheduleModel> importModelList = new();
                        List<string> errorMessageList = new();

                        var fileName = file.FileName;

                        if (file.Length > 0)
                        {
                            var csvInputFile = new CsvFileInputModel()
                            {
                                FileName = file.FileName,
                                ImportFile = file,
                                HeaderColumnCount = Header_Column_Count,
                                HeaderSettings = GetModelHeaderCheck()
                            };

                            // CSVファイルデータ読み取り
                            var (readCsvErrorMsg, lines, newFilePath) = await ReadFile.ReadCsv(csvInputFile, GamenName);
                            tempFilePath = newFilePath;

                            if (!string.Empty.Equals(readCsvErrorMsg))
                            {
                                //ファイルを削除
                                ReadFile.DeleteFile(tempFilePath);
                                // エラーメッセージ取得
                                return NotFound(new { errorMessage = readCsvErrorMsg });
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

                                // データチェック
                                if (readCount > 0)
                                {
                                    // 読み取りデータをモデルに設定
                                    receiveSchedule = SetReadDataInModel(receiveSchedule, lines, readCount);

                                    // 入荷予定データチェック
                                    var validationContext = new ValidationContext(receiveSchedule);
                                    var validationResults = new List<ValidationResult>();
                                    bool isValid = Validator.TryValidateObject(receiveSchedule, validationContext, validationResults, true);

                                    // 会社コードで会社IDを取得
                                    var companyId = M_CompanyConnectController.GetCompanyIdByCompanyCode(receiveSchedule.CompanyCode, user.DatabaseName);
                                    if (companyId == -1)
                                    {
                                        isValid = false;
                                        var message = string.Format(ErrorMessagesResources.E1011, Utils.GetDisplayName<D_ReceiveScheduleModel>("CompanyCode"));
                                        validationResults.Add(new ValidationResult(message, new List<string> { "CompanyCode" }));
                                    }
                                    receiveSchedule.CompanyID = companyId;

                                    // 仕入先品番で品番チェック
                                    bool isExistProduct = M_ProductConnectController.CheckMProductExist(receiveSchedule.SupplierProductNumber, user.DatabaseName);
                                    if (!isExistProduct)
                                    {
                                        isValid = false;
                                        var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"));
                                        validationResults.Add(new ValidationResult(message, new List<string> { "SupplierProductNumber" }));
                                    }

                                    // エラーメッセージを追加
                                    if (!isValid)
                                    {
                                        foreach(var err in validationResults)
                                        {
                                            // フォーマットエラーメッセージ
                                            List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ReceiveScheduleModel>(err);
                                            errorMessageItem.Insert(0, readCount + "行目");
                                            
                                            //HTMLに変換
                                            var errorHtml = string.Empty;
                                            foreach(var item in errorMessageItem)
                                            {
                                                errorHtml += "<td class='pl-2 pr-2'>" + item.ToString() + "</td>";
                                            }
                                            errorHtml = "<tr>" + errorHtml + "</tr>";

                                            errorMessageList.Add(errorHtml);
                                        }
                                    }
                                    // リストに項目を追加
                                    importModelList.Add(receiveSchedule);
                                }
                                readCount++;
                            }
                        }

                        // エラーが1件以上ある場合はreturn
                        if (errorMessageList.Count > 0)
                        {
                            //ファイルを削除
                            ReadFile.DeleteFile(tempFilePath);
                            var errorMessage = string.Join("</br>", errorMessageList);
                            return NotFound(new { errorMessage });
                        }

                        // 入荷予定データ書き込み
                        bool insertResult = D_ReceiveScheduleConnectController.InsertDReceiveSchedule(importModelList, DepoID, fileName, user);
                        if (!insertResult)
                        {
                            //ファイルを削除
                            ReadFile.DeleteFile(tempFilePath);
                            return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
                        }
                    }
                }
                else
                {
                    return NotFound(new { errorMessage = "E1012: " + ErrorMessagesResources.E1012 });
                }

                return Ok();
            }
            catch (SqlException)
            {
                //ファイルを削除
                ReadFile.DeleteFile(tempFilePath);
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                //ファイルを削除
                ReadFile.DeleteFile(tempFilePath);
                return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// モデルヘッダー名リスト取得
        /// </summary>
        private Dictionary<int, string> GetModelHeaderCheck()
        {
            Dictionary<int, string> headerSettings = new()
            {
                [0] = Utils.GetDisplayName<D_ReceiveScheduleModel>("CompanyCode"),
                [1] = Utils.GetDisplayName<D_ReceiveScheduleModel>("ReceiveScheduleDate"),
                [2] = Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"),
                [3] = Utils.GetDisplayName<D_ReceiveScheduleModel>("LotNumber"),
                [4] = Utils.GetDisplayName<D_ReceiveScheduleModel>("Quantity")
            };

            return headerSettings;
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
                ReadFile.ToCSV(dataTable, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception)
            {
                return Json(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
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
