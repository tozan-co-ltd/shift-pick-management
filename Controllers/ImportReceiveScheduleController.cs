using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 入荷予定取込画面
    /// </summary>
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
            D_FileImportModel model = new ();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    ViewData["ErrorMessage"] = "E1015: " + ErrorMessagesResources.E1015;
                    return View(model);
                }

                string controllerName = ControllerContext.ActionDescriptor.ControllerName;
                CommonModel commonModel = new ()
                {
                    ControllerName = controllerName,
                    CompanyID = user.CompanyID
                };
                // SQL作成
                var sql = D_FileImportConnectController.CreateSQLToSelectDFileImports(commonModel.GetViewTitle());
                // DB接続
                List<D_FileImportModel> dFileImportList = D_FileImportConnectController.ConnectDFileImports(sql, user.DatabaseName);
                model.D_FileImportList = dFileImportList;

                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// CSV取込
        /// </summary>
        /// <param name="FileUpload"></param>
        /// <param name="DepoID"></param>
        /// <param name="GamenName"></param>
        /// <returns></returns>
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
                    return NotFound(new { errorMessage = "E1015: " + ErrorMessagesResources.E1015 });
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
                            var (readCsvErrorMsg, lines, newFilePath) = await CreateFile.ReadCsv(csvInputFile, GamenName);
                            tempFilePath = newFilePath;

                            if (!string.Empty.Equals(readCsvErrorMsg))
                            {
                                // ファイル削除
                                CreateFile.DeleteFile(tempFilePath);
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
                                    List<string> errorMembers = validationResults.SelectMany(result => result.MemberNames).Distinct().ToList();
                                    
                                    // 会社コードチェック
                                    bool isContainCompanyCode = errorMembers.Contains("CompanyCode");
                                    if (!isContainCompanyCode)
                                    {
                                        // 会社コードで会社IDを取得
                                        var companyId = M_CompanyConnectController.GetCompanyIdByCompanyCode(receiveSchedule.CompanyCode, user.DatabaseName);
                                        if (companyId == -1)
                                        {
                                            isValid = false;
                                            var message = string.Format(ErrorMessagesResources.E1011, Utils.GetDisplayName<D_ReceiveScheduleModel>("CompanyCode"));
                                            validationResults.Add(new ValidationResult(message, new List<string> { "CompanyCode" }));
                                        }
                                        receiveSchedule.CompanyID = companyId;
                                    }

                                    // 仕入先品番チェック
                                    bool isContainSupplierProductNumber = errorMembers.Contains("SupplierProductNumber");
                                    if (!isContainSupplierProductNumber)
                                    {
                                        // 仕入先品番で品番チェック
                                        bool isExistProduct = M_ProductConnectController.IsExistedSupplierProductNumber(receiveSchedule.SupplierProductNumber, user.DatabaseName);
                                        if (!isExistProduct)
                                        {
                                            isValid = false;
                                            var message = string.Format(ErrorMessagesResources.E1010, Utils.GetDisplayName<D_ReceiveScheduleModel>("SupplierProductNumber"));
                                            validationResults.Add(new ValidationResult(message, new List<string> { "SupplierProductNumber" }));
                                        }
                                    }

                                    // エラーメッセージ作成
                                    if (!isValid)
                                    {
                                        foreach(var err in validationResults)
                                        {
                                            // フォーマットエラーメッセージ
                                            List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ReceiveScheduleModel>(err);
                                            errorMessageItem.Insert(0, readCount + "行目");
                                            
                                            // HTMLに変換
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
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);
                            var errorMessage = string.Join("</br>", errorMessageList);
                            return NotFound(new { errorMessage });
                        }

                        // 入荷予定データ書き込み
                        bool insertResult = D_ReceiveScheduleConnectController.InsertDReceiveSchedule(importModelList, DepoID, fileName, user);
                        if (!insertResult)
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);
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
                // ファイル削除
                CreateFile.DeleteFile(tempFilePath);
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                // ファイル削除
                CreateFile.DeleteFile(tempFilePath);
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
                CreateFile.ConvertDataTableToCsv(dataTable, filePath);
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
