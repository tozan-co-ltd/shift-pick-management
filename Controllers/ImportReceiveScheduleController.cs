using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ai_truck_load_measurement.Controllers
{
    /// <summary>
    /// 入荷予定取込画面
    /// </summary>
    public class ImportReceiveScheduleController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ヘッダー列数取得
        /// </summary>
        public readonly int Header_Column_Count = 5;

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

                // 入荷予定情報取得
                var sql = D_FileImportConnectController.CreateSQLToSelectDFileImports(commonModel.GetViewTitle());
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
        /// <param name="uploadFileList"></param>
        /// <param name="depoId"></param>
        /// <param name="viewTitle"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ImportCsv(List<IFormFile> uploadFileList, int depoId, string viewTitle)
        {
            string? errorMessage;
            string tempFilePath = string.Empty;
            try
            {
                var files = uploadFileList;

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    return NotFound(new { errorMessage = "E1015: " + ErrorMessagesResources.E1015 });
                }

                // log取得
                _logger.Info($"入荷予定取込開始 ログインユーザー名:{user.UserName}");

                // モデルリスト取得
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        List<D_ReceiveScheduleModel> importModelList = new();
                        List<string> errorMessageList = new();
                        var fileName = file.FileName;

                        // log取得
                        _logger.Info($"ファイル名:{fileName}");

                        // ファイル内にデータがない場合はエラー
                        if (file.Length == 0)
                        {
                            // log取得
                            errorMessage = "E1014: " + ErrorMessagesResources.E1014;
                            _logger.Error($"取込失敗 {errorMessage}");

                            return NotFound(new { errorMessage });
                        }

                        CsvFileInputModel csvInputFile = new ()
                        {
                            FileName = file.FileName,
                            ImportFile = file,
                            HeaderColumnCount = Header_Column_Count,
                            HeaderSettings = GetModelHeaderCheck()
                        };

                        // CSVファイルデータ読み取り
                        var (readCsvErrorMsg, lines, newFilePath) = await CreateFile.ReadCsv(csvInputFile, viewTitle);
                        tempFilePath = newFilePath;

                        if (!string.Empty.Equals(readCsvErrorMsg))
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);

                            // log取得
                            _logger.Error($"データ読み取り失敗 {readCsvErrorMsg}");

                            return NotFound(new { errorMessage = readCsvErrorMsg });
                        }

                        // 読み取りデータを更新
                        int readCount = 1;
                        while (readCount < lines.Count)
                        {
                            D_ReceiveScheduleModel receiveSchedule = new()
                            {
                                ImportFileName = fileName,
                                SelectedDepoID = depoId
                            };

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
                                foreach (var err in validationResults)
                                {
                                    // フォーマットエラーメッセージ
                                    List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ReceiveScheduleModel>(err);
                                    errorMessageItem.Insert(0, string.Concat(readCount + 1, "行目"));

                                    // HTMLに変換
                                    var errorHtml = string.Empty;
                                    foreach (var item in errorMessageItem)
                                    {
                                        errorHtml += "<td class='pl-2 pr-2'>" + item.ToString() + "</td>";
                                    }
                                    errorHtml = "<tr>" + errorHtml + "</tr>";

                                    errorMessageList.Add(errorHtml);
                                }
                            }
                            // リストに項目を追加
                            importModelList.Add(receiveSchedule);

                            readCount++;
                        }

                        // エラーが1件以上ある場合はreturn
                        if (errorMessageList.Count > 0)
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);
                            errorMessage = string.Join("</br>", errorMessageList);

                            // log取得
                            _logger.Error($"取込失敗");

                            return NotFound(new { errorMessage });
                        }

                        // 入荷予定データ書き込み
                        bool insertResult = D_ReceiveScheduleConnectController.InsertDReceiveSchedule(importModelList, depoId, fileName, viewTitle, user);
                        if (!insertResult)
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);

                            // log取得
                            errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                            _logger.Error($"データ書き込み失敗 {errorMessage}");

                            return NotFound(new { errorMessage });
                        }
                    }
                }
                else
                {
                    // log取得
                    errorMessage = "E1012: " + ErrorMessagesResources.E1012;
                    _logger.Error($"ファイルなし {errorMessage}");

                    return NotFound(new { errorMessage = "E1012: " + ErrorMessagesResources.E1012 });
                }

                // log取得
                _logger.Info($"取込完了");

                return Ok();
            }
            catch (SqlException ex)
            {
                // ファイル削除
                CreateFile.DeleteFile(tempFilePath);

                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // ファイル削除
                CreateFile.DeleteFile(tempFilePath);

                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
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
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // テーブルデータ取得
                DataTable dataTable = CreateDataTable();

                // ファイル名
                var tmpFilename = "入荷予定.csv";

                // CSVファイルへのパスを作成
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換
                CreateFile.ConvertDataTableToCsv(dataTable, filePath);
                // ファイル作成
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
