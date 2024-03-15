using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Transactions;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// 出荷指示取込画面
    /// </summary>
    public class ImportShipmentScheduleController : BaseController
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ヘッダー列数取得
        /// </summary>
        public readonly int Header_Column_Count = 45;

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

                // 出荷指示情報取得
                var sql = D_FileImportConnectController.CreateSQLToSelectDFileImports(commonModel.GetViewTitle());
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
        /// <param name="uploadFileList"></param>
        /// <param name="depoId"></param>
        /// <param name="companyId"></param>
        /// <param name="viewTitle"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ImportCsv(List<IFormFile> uploadFileList, int depoId, int companyId, string viewTitle)
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
                _logger.Info($"出荷指示取込開始 ログインユーザー名:{user.UserName}");

                // モデルリスト取得
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        List<D_ShipmentScheduleModel> importModelList = new();
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
                            D_ShipmentScheduleModel shipmentSchedule = new()
                            {
                                ImportFileName = fileName,
                                DepoID = depoId,
                                CompanyID= companyId,
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
                                // 品番マスター・倉庫-品番中間テーブルに登録されている品番の行のみ取り込む
                                // 登録されていない品番の行はスキップする
                                var product = M_ProductConnectController.GetProductByShipmentDeliveryProductNumber(
                                    shipmentSchedule.CompanyID, shipmentSchedule.DepoID, shipmentSchedule.DeliveryProductNumber, user.DatabaseName
                                );
                                if (product == null)
                                {
                                    readCount++;

                                    // log取得
                                    _logger.Info($"登録されていない品番の行はスキップ 納入先ID:{shipmentSchedule.CompanyID}, 倉庫ID{shipmentSchedule.DepoID}, 納入先品番:{shipmentSchedule.DeliveryProductNumber}");

                                    continue;
                                }
                                shipmentSchedule.SupplierProductNumber = product.SupplierProductNumber;
                            }

                            // 出荷実績または出庫実績がある場合はエラー
                            string checkShipmentSql = D_ShipmentScheduleConnectController.CreateSQLToIsExistDShipment(shipmentSchedule, depoId, companyId);
                            bool isExistedShipment = ConnectToSQLServer.IsExistedSameRecord(checkShipmentSql, user.DatabaseName);
                            string checkStoreOutSql = D_ShipmentScheduleConnectController.CreateSQLToCheckExistDStoreOutByShipmentSchedule(shipmentSchedule);
                            bool isExistedStoreOut = ConnectToSQLServer.IsExistedSameRecord(checkStoreOutSql, user.DatabaseName);
                            if (isExistedShipment || isExistedStoreOut)
                            {
                                isValid = false;
                                validationResults.Add(new ValidationResult(ErrorMessagesResources.E1020, new List<string> { "ShipmentScheduleID" }));
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

                        // 登録データが存在するかチェック
                        if (importModelList.Count == 0)
                        {
                            // ファイル削除
                            CreateFile.DeleteFile(tempFilePath);

                            // log取得
                            errorMessage = "E1014: " + ErrorMessagesResources.E1014;
                            _logger.Error($"登録データなし {errorMessage}");

                            return NotFound(new { errorMessage });
                        }

                        // 出荷指示データ書き込み
                        bool insertResult = D_ShipmentScheduleConnectController.InsertDShipmentSchedule(importModelList, depoId, companyId, fileName, viewTitle, user);
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

                    return NotFound(new { errorMessage });
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
        private static Dictionary<int, string> GetModelHeaderCheck()
        {
            Dictionary<int, string> headerSettings = new()
            {
                [6] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererCode"),
                [7] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryKubun"),
                [8] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererName"),
                [9] = Utils.GetDisplayName<D_ShipmentScheduleModel>("OrdererFactoryName"),
                [10] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperCode"),
                [11] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperFactoryKubun"),
                [13] = Utils.GetDisplayName<D_ShipmentScheduleModel>("ShipperName"),
                [16] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryCode"),
                [17] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryKubun"),
                [18] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryLocation"),
                [19] = Utils.GetDisplayName<D_ShipmentScheduleModel>("NameOfDelivery"),
                [20] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryFactoryName"),
                [22] = Utils.GetDisplayName<D_ShipmentScheduleModel>("RegularKubun"),
                [24] = Utils.GetDisplayName<D_ShipmentScheduleModel>("IssuedDate"),
                [25] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryDate"),
                [26] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTime"),
                [27] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryTimeClass"),
                [28] = Utils.GetDisplayName<D_ShipmentScheduleModel>("TranspotationIdentify"),
                [30] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipNumber"),
                [31] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipPageNumber"),
                [32] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliverySlipRowNumber"),
                [34] = "表示用品番",
                [35] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductAbbreviation"),
                [36] = Utils.GetDisplayName<D_ShipmentScheduleModel>("DeliveryProductName"),
                [37] = Utils.GetDisplayName<D_ShipmentScheduleModel>("LotQuantity"),
                [43] = Utils.GetDisplayName<D_ShipmentScheduleModel>("BranchNumber"),
                [44] = Utils.GetDisplayName<D_ShipmentScheduleModel>("Quantity")
            };

            return headerSettings;
        }

        /// <summary>
        /// 読み取りデータをモデルに設定
        /// </summary>
        private static D_ShipmentScheduleModel SetReadDataInModel(D_ShipmentScheduleModel model, List<string[]> lines, int readCount)
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
            model.NameOfDelivery = lines[readCount][19];
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
                    model.NumberOfBoxes = (int)Math.Ceiling((double)quantity / lotQuantity);
                }
            }

            return model;
        }
    }
}
