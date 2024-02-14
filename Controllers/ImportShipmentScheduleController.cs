using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using X.PagedList;

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
                // log取得
                var exceptionMessage = ex.Message;

                var d_FileImportModel = new D_FileImportModel
                {
                    Message = exceptionMessage
                };
                return View(d_FileImportModel);
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

                                    if (!isValid)
                                    {
                                        // エラーメッセージ取得
                                        return NotFound(new { errorMessage = ErrorMessagesResources.E9999 });
                                    }

                                    importModelList.Add(shipmentSchedule);
                                }
                                readCount++;
                            }
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
            headerSettings[0] = "発注者";
            headerSettings[1] = "発注者事業所";
            headerSettings[2] = "受注者";
            headerSettings[3] = "受注者事業所";
            headerSettings[4] = "品番";
            headerSettings[5] = "部品取扱識別";
            headerSettings[6] = "発注元";
            headerSettings[7] = "発注元工区";
            headerSettings[8] = "発注元名称";
            headerSettings[9] = "発注元工場名";
            headerSettings[10] = "出荷元";
            headerSettings[11] = "出荷元工区";
            headerSettings[13] = "出荷元名称";
            headerSettings[22] = "定期／不定期区分名称";
            headerSettings[24] = "発行日";
            headerSettings[25] = "納入指示日";
            headerSettings[26] = "納入指示時刻";
            headerSettings[27] = "便";
            headerSettings[28] = "輸送識別";
            headerSettings[30] = "納品書番号";
            headerSettings[31] = "ページ数";
            headerSettings[32] = "行No";
            headerSettings[34] = "表示用品番";
            headerSettings[35] = "背番号";
            headerSettings[36] = "品名";
            headerSettings[37] = "収容数";
            headerSettings[43] = "枝番";
            headerSettings[44] = "納入指示数";

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

            model.IssuedDate = DateTime.ParseExact(lines[readCount][24], Utils.DateFormats, CultureInfo.InvariantCulture);
            model.DeliveryDate = DateTime.ParseExact(lines[readCount][25], Utils.DateFormats, CultureInfo.InvariantCulture);
            model.DeliveryTime = lines[readCount][26];
            model.DeliveryTimeClass = Convert.ToInt32(lines[readCount][27]);
            model.TranspotationIdentify = lines[readCount][28];

            model.DeliverySlipNumber = lines[readCount][30];
            model.DeliverySlipPageNumber = Convert.ToInt32(lines[readCount][31]);
            model.DeliverySlipRowNumber = Convert.ToInt32(lines[readCount][32]);

            model.DeliveryProductNumber = lines[readCount][34];
            model.DeliveryProductAbbreviation = lines[readCount][35];
            model.DeliveryProductName = lines[readCount][36];
            model.LotQuantity = Convert.ToInt32(lines[readCount][37]);

            model.BranchNumber = Convert.ToInt32(lines[readCount][43]);
            model.Quantity = Convert.ToInt32(lines[readCount][44]);

            // 箱数＝納入指示数/収容数
            if (model.Quantity == 0 || model.LotQuantity == 0)
            {
                model.NumberOfBoxes = 0;
            }
            else
            {
                model.NumberOfBoxes = model.Quantity / model.LotQuantity;
            }

            return model;
        }
    }
}
