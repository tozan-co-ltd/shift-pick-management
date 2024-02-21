using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace mar_sumaken_web.Controllers
{
    public class ImportShipmentScheduleController : BaseController
    {
        private readonly ILogger<ImportShipmentScheduleController> _logger;

        /// <summary>
        /// ヘッダー列数取得
        /// </summary>
        public readonly int Header_Column_Count = 50;

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
                var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport("出荷指示取込");

                // DB接続
                List<D_FileImportModel> listD_FileImport = D_FileImportConnectController.ConnectD_FileImport(sql, user.DatabaseName);

                model.D_FileImportList = listD_FileImport;

                return View(model);
            }
            catch (Exception)
            {
                return View();
            }
        }

        // <summary>
        /// Csv取込
        /// <param name="FileUpload">ファイル</param>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportCsv(List<IFormFile> FileUpload, int DepoID, int CompanyID, string GamenName)
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
                    return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
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
                            var (readCsvErrorMsg, lines) = await ReadFile.ReadCsv(csvInputFile, GamenName, user.UserID);

                            if (!string.Empty.Equals(readCsvErrorMsg))
                            {
                                // エラーメッセージ取得
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

                                // 読み取りデータをモデルに設定
                                shipmentSchedule = SetReadDataInModel(shipmentSchedule, lines, readCount);

                                // 納入先品番で仕入先品番を取得
                                // 品番マスターに登録されている品番の行のみ取り込まれます。登録されていない品番の行はスキップします。
                                var product = M_ProductConnectController.GeProductByDeliveryProductNumber(
                                    shipmentSchedule.SelectedCompanyID,  shipmentSchedule.DeliveryProductNumber, user.DatabaseName
                                );
                                if (product == null)
                                {
                                    readCount++;
                                    continue;
                                }
                                shipmentSchedule.SupplierProductNumber = product.SupplierProductNumber;

                                // 倉庫-品番中間テーブルチェック
                                bool isExist = M_ProductConnectController.IsExistRDepoProduct(product.ProductID, DepoID, user.DatabaseName);
                                if (!isExist)
                                {
                                    readCount++;
                                    continue;
                                }

                                // 出荷指示データチェック
                                var validationContext = new ValidationContext(shipmentSchedule);
                                var validationResults = new List<ValidationResult>();
                                bool isValid = Validator.TryValidateObject(shipmentSchedule, validationContext, validationResults, true);

                                // エラーメッセージを追加
                                if (!isValid)
                                {
                                    foreach (var err in validationResults)
                                    {
                                        var msg = readCount + "行目" + "　" + err.ErrorMessage;
                                        errorMessageList.Add(msg);
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
                            var errorMessage = string.Join("</br>", errorMessageList);
                            return NotFound(new { errorMessage });
                        }

                        // 登録データが存在するかチェック
                        if (importModelList.Count == 0)
                        {
                            return NotFound(new { errorMessage = "E1014: " + ErrorMessagesResources.E1014 });
                        }

                        // 出荷指示データ書き込み
                        bool insertResult = D_ShipmentScheduleConnectController.InsertDShipmentSchedule(importModelList, DepoID, CompanyID, fileName, user);
                        if (!insertResult)
                        {
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
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
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
