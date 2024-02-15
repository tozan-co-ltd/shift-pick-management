using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Reflection;

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
            try
            {
                var model = new D_FileImportModel();

                var listD_FileImport = GetListD_FileImport(ClaimsLoginUserData().DatabaseName);
                model.D_FileImportList = listD_FileImport;

                return View(model);
            }
            catch (Exception ex)
            {
                return View();
                //var exceptionMessage = ex.Message;

                //var d_FileImportModel = new D_FileImportModel
                //{
                //    Message = exceptionMessage
                //};
                //return View(d_FileImportModel);
            }
        }

        /// <summary>
        /// 出荷指示取込一覧取得
        /// </summary>
        /// <param name="databaseName">string</param>
        /// <returns>出庫実績情報</returns>
        public List<D_FileImportModel> GetListD_FileImport(string databaseName)
        {
            // SQL作成
            var sql = D_FileImportConnectController.CreateSQLToGetD_FileImport("出荷指示取込");

            // DB接続
            List<D_FileImportModel> strList = D_FileImportConnectController.ConnectD_FileImport(sql, databaseName);

            return strList;
        }

        // <summary>
        /// Excel取込
        /// <param name="FileUpload">ファイル</param>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportExcel(List<IFormFile> FileUpload, int DepoID, int CompanyID)
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
                        List<D_ShipmentScheduleModel> importModelList = new();
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
                            string importFilePath = Utils.CreateImportFilePath(fileName, user.UserID, "ShipmentSchedule");

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
                                return NotFound(new { errorMessage = "正しいファイルを指定してください。" });
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
                                D_ShipmentScheduleModel shipmentSchedule = new();
                                shipmentSchedule.ImportFileName = fileName;

                                // 倉庫ID、納入先ID取得
                                shipmentSchedule.SelectedDepoID = DepoID;
                                shipmentSchedule.SelectedCompanyID = CompanyID;

                                // ヘッダー名チェック
                                if (readCount == 0)
                                {
                                    bool isValidHeader = CheckIsValidHeader(lines[0]);
                                    if (!isValidHeader)
                                    {
                                        // エラーメッセージ取得
                                        return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                                    }
                                }

                                // データチェック
                                if (readCount > 0)
                                {
                                    // 読み取りデータをモデルに設定
                                    shipmentSchedule = SetReadDataInModel(shipmentSchedule, lines, readCount);

                                    // 出荷指示データチェック
                                    var validationContext = new ValidationContext(shipmentSchedule);
                                    var validationResults = new List<ValidationResult>();
                                    bool isValid = Validator.TryValidateObject(shipmentSchedule, validationContext, validationResults, true);

                                    // エラーがあります。
                                    if (!isValid)
                                    {
                                        foreach (var err in validationResults)
                                        {
                                            var msg = readCount + "行目" + "　" + err.ErrorMessage;
                                            errorList.Add(msg);
                                        }
                                    }
                                    // リストに項目を追加
                                    importModelList.Add(shipmentSchedule);
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

                        // 出荷指示データ書き込み
                        bool insertResult = D_ShipmentScheduleConnectController.InsertDShipmentSchedule(importModelList, DepoID, CompanyID, fileName, user);
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
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                // log取得
                var exceptionMessage = ex.Message;
                _logger.LogError($"{exceptionMessage} {ErrorMessagesResources.E9999}");
                return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// ヘッダー名チェック
        /// </summary>
        /// <param name="headerCheck">チェックされたヘッダー</param>
        private bool CheckIsValidHeader(string[] headerCheck)
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
            var quantityValue = Convert.ToInt32(model.Quantity);
            var lotQuantity = Convert.ToInt32(model.LotQuantity);
            if (quantityValue == 0 || lotQuantity == 0)
            {
                model.NumberOfBoxes = 0;
            }
            else
            {
                model.NumberOfBoxes = quantityValue / lotQuantity;
            }

            return model;
        }
    }
}
