using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出荷指示取込画面
    /// </summary>
    public class ImportShipmentScheduleController : BaseController
    {
        private readonly ILogger<ImportShipmentScheduleController> _logger;

        /// <summary>
        /// ヘッダー列数取得
        /// </summary>
        public readonly int Header_Column_Count = 45;

        public ImportShipmentScheduleController(ILogger<ImportShipmentScheduleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 出荷指示取込画面表示
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

                // 会社リスト取得
                model.SearchCompanyList = commonModel.GetMCompanyList(user.DatabaseName, Utils.Const_DeliveryID);

                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// CSV取込
        /// </summary>
        /// <param name="FileUpload"></param>
        /// <param name="DepoID"></param>
        /// <param name="CompanyID"></param>
        /// <param name="GamenName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ImportCsv(List<IFormFile> FileUpload, int DepoID, int CompanyID, string GamenName)
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
                        List<D_ShipmentScheduleModel> importModelList = new();
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
                            int readCount = 1;
                            while (readCount < lines.Count)
                            {
                                D_ShipmentScheduleModel shipmentSchedule = new()
                                {
                                    ImportFileName = fileName,
                                    SelectedDepoID = DepoID,
                                    SelectedCompanyID = CompanyID
                                };
                                var validationContext = new ValidationContext(shipmentSchedule);
                                var validationResults = new List<ValidationResult>();

                                // 読み取りデータをモデルに設定
                                shipmentSchedule = SetReadDataInModel(shipmentSchedule, lines, readCount);

                                // 出荷指示データチェック
                                bool isValid = Validator.TryValidateObject(shipmentSchedule, validationContext, validationResults, true);
                                List<string> errorMembers = validationResults.SelectMany(result => result.MemberNames).Distinct().ToList();

                                // 納入先品番チェック
                                bool isContainDeliveryProductNumber = errorMembers.Contains("DeliveryProductNumber");
                                if (!isContainDeliveryProductNumber)
                                {
                                    // 納入先品番で仕入先品番を取得
                                    // 品番マスターに登録されている品番の行のみ取り込まれます。登録されていない品番の行はスキップします。
                                    var product = M_ProductConnectController.GetProductByDeliveryProductNumber(
                                        shipmentSchedule.SelectedCompanyID, shipmentSchedule.DeliveryProductNumber, user.DatabaseName
                                    );
                                    if (product == null)
                                    {
                                        readCount++;
                                        continue;
                                    }
                                    shipmentSchedule.SupplierProductNumber = product.SupplierProductNumber;

                                    // 倉庫-品番中間テーブルチェック
                                    var sql = M_ProductConnectController.CreateSQLToCheckIsExistRDepoProduct(DepoID, product.ProductID);
                                    bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                                    if (!isExisted)
                                    {
                                        readCount++;
                                        continue;
                                    }
                                }

                                // エラーメッセージ作成
                                if (!isValid)
                                {
                                    foreach (var error in validationResults)
                                    {
                                        // フォーマットエラーメッセージ
                                        List<string> errorMessageItem = Utils.FormatValidationErrorMessage<D_ShipmentScheduleModel>(error);
                                        errorMessageItem.Insert(0, readCount + "行目");

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
                                importModelList.Add(shipmentSchedule);

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

                        // 登録データが存在するかチェック
                        if (importModelList.Count == 0)
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);
                            return NotFound(new { errorMessage = "E1014: " + ErrorMessagesResources.E1014 });
                        }

                        // 出荷指示データ書き込み
                        bool insertResult = D_ShipmentScheduleConnectController.InsertDShipmentSchedule(importModelList, DepoID, CompanyID, fileName, user);
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
            Dictionary<int, string> headerSettings = new Dictionary<int, string>();
            headerSettings[6] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererCode");
            headerSettings[7] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryKubun");
            headerSettings[8] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererName");
            headerSettings[9] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryName");
            headerSettings[10] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperCode");
            headerSettings[11] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperFactoryKubun");
            headerSettings[13] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperName");
            headerSettings[16] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryCode");
            headerSettings[17] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryKubun");
            headerSettings[18] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryLocation");
            headerSettings[19] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryName");
            headerSettings[20] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryName");
            headerSettings[22] = Utils.GetDisplayName<D_ShipmentScheduleModel>("RegularKubun");
            headerSettings[24] = Utils.GetDisplayName<D_ShipmentScheduleModel>("IssuedDate");
            headerSettings[25] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryDate");
            headerSettings[26] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTime");
            headerSettings[27] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTimeClass");
            headerSettings[28] = Utils.GetDisplayName<D_ShipmentScheduleModel>("TranspotationIdentify");
            headerSettings[30] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipNumber");
            headerSettings[31] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipPageNumber");
            headerSettings[32] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipRowNumber");
            headerSettings[34] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductNumber");
            headerSettings[35] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductAbbreviation");
            headerSettings[36] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductName");
            headerSettings[37] = Utils.GetDisplayName<D_ShipmentScheduleModel>("LotQuantity");
            headerSettings[43] = Utils.GetDisplayName<D_ShipmentScheduleModel>("BranchNumber");
            headerSettings[44] = Utils.GetDisplayName<D_ShipmentScheduleModel>("Quantity");

            return headerSettings;
        }

        /// <summary>
        /// 読み取りデータをモデルに設定
        /// </summary>
        private D_ShipmentScheduleModel SetReadDataInModel(D_ShipmentScheduleModel model, List<string[]> lines, int readCount)
        {
            model.OrdererCode = lines[readCount][6];
            model.OrdererFactoryKubun = lines[readCount][7];
            model.OrdererName = lines[readCount][8];
            model.OrdererFactoryName = lines[readCount][9];
            model.ShipperCode = lines[readCount][10];
            model.ShipperFactoryKubun = lines[readCount][11];
            model.ShipperName = lines[readCount][13];

            model.DeliveryCode = lines[readCount][16];
            model.DeliveryFactoryKubun = lines[readCount][17];
            model.DeliveryLocation = lines[readCount][18];
            model.DeliveryName = lines[readCount][19];
            model.DeliveryFactoryName = lines[readCount][20];

            model.RegularKubun = lines[readCount][22];

            model.IssuedDate = lines[readCount][24];
            model.DeliveryDate = lines[readCount][25];
            model.DeliveryTime = lines[readCount][26];
            model.DeliveryTimeClass = lines[readCount][27];
            model.TranspotationIdentify = lines[readCount][28];

            model.DeliverySlipNumber = lines[readCount][30];
            model.DeliverySlipPageNumber = lines[readCount][31];
            model.DeliverySlipRowNumber = lines[readCount][32];

            model.DeliveryProductNumber = lines[readCount][34];
            model.DeliveryProductAbbreviation = lines[readCount][35];
            model.DeliveryProductName = lines[readCount][36];
            model.LotQuantity = lines[readCount][37];

            model.BranchNumber = lines[readCount][43];
            model.Quantity = lines[readCount][44];

            // 箱数＝納入指示数/収容数
            bool isValidQuantity = int.TryParse(model.Quantity, out int quantity);
            bool isValidLotQuantity = int.TryParse(model.LotQuantity, out int lotQuantity);
            if (isValidQuantity && isValidLotQuantity)
            {
                if (quantity == 0 || lotQuantity == 0)
                {
                    model.NumberOfBoxes = 0;
                }
                else
                {
                    model.NumberOfBoxes = quantity / lotQuantity;
                }
            }

            return model;
        }
    }
}
