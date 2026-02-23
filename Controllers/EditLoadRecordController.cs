using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class EditLoadRecordController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            LoadRecordViewModel model = new();

            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();
            // メインデポ情報取得
            model = LoadRecordController.SetMainDepoInfo(model, user);
            try
            {
                // 便実績情報取得SQL作成
                var sql = LoadRecordConnectController.CreatSQLToSelectTripRecord();
                // DB接続
                IEnumerable<LoadOutputModel> tripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadOutputModel>(sql);
                // テーブル情報を変換
                tripRecordList = (IEnumerable<LoadOutputModel>)LoadRecordController.ConversionForTable(tripRecordList);

                model.TripRecordList = tripRecordList.ToPagedList();
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
        /// 便実績情報テーブル非同期更新用
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <param name="isOnlyHasAmountDefference">荷量の相違ありのみ表示か</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, bool isOnlyHasAmountDefference, bool hasTripName, bool hasIdentifyNumber, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            IEnumerable<LoadRecordModel> tripRecordList;
            // DB接続
            var sql = EditLoadRecordConnectController.CreatSQLToSelectTripRecordFromPeriod(startOfPeriod, endOfPeriod, isOnlyHasAmountDefference, hasTripName, hasIdentifyNumber, checkedDepos);
            tripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
            // 荷量のクラスを数値に、画像パスをBase64に変換
            tripRecordList = LoadRecordController.ConversionForTable(tripRecordList);
            searchData += $@"
                <div class=""mt-3"">
                    <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                        <thead>
                            <tr align=""center"">
                                <th class=""font-weight-bold"">実績<br>修正</th>
                                <th class=""font-weight-bold"">便名称</th>
                                <th class=""font-weight-bold"">便枝番</th>
                                <th class=""font-weight-bold"">タグ</th>
                                <th hidden>便実績ID</th>
                                <th hidden>便名称有無</th>
                                <th class=""font-weight-bold"">識別<br>番号</th>
                                <th class=""font-weight-bold"">到着実績</th>
                                <th class=""font-weight-bold"">出発実績</th>
                                <th class=""font-weight-bold"">到着荷量<br>(%)</th>
                                <th class=""font-weight-bold"">出発荷量<br>(%)</th>
                                <th class=""font-weight-bold"">到着荷量<br>画像</th>
                                <th class=""font-weight-bold"">出発荷量<br>画像</th>
                                <th class=""font-weight-bold"">乗務員</th>
                                <th class=""font-weight-bold"">ステーション<br>名</th>
                                <th class=""font-weight-bold"">車両<br>番号</th>
                                <th class=""font-weight-bold"">デポ</th>
                                <th class=""font-weight-bold"">到着<br>予定</th>
                                <th class=""font-weight-bold"">出発<br>予定</th>
                                <th class=""font-weight-bold"">稼働日</th>
                                <th class=""font-weight-bold"">紐づけ切れ理由</th>
                            </tr>
                        </thead>
                        <tbody>
            ";
            // 新しい便情報テーブルのhtml作成
            if (tripRecordList.Count() > 0)
            {
                foreach (var item in tripRecordList)
                {
                    var truckNumber = item.TruckNumber;
                    if (truckNumber == "0") truckNumber = "-";
                    var arrivalScheduledTime = item.ArrivalScheduledTime.ToString("HH:mm");
                    if (arrivalScheduledTime == "00:00") arrivalScheduledTime = "-";
                    var departureScheduledTime = item.DepartureScheduledTime.ToString("HH:mm");
                    if (departureScheduledTime == "00:00") departureScheduledTime = "-";
                    var notHasTripName = 0;
                    if (item.TripName == "-") notHasTripName = 1;
                    var departed = item.DepartedAt.ToString("yyyy/MM/dd HH:mm");
                    if (departed == "0001/01/01 00:00") departed = "-";
                    var unlinkedReason = item.UnlinkedReasonName;
                    if (string.IsNullOrEmpty(unlinkedReason)) unlinkedReason = "-";
                    searchData += $@"
                        <tr>
                            <td>
                                <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                    onclick=""onRegisterClick('EditLoadRecord', '{item.TripRecordID}')"" data-id=""{item.TripRecordID}"" >
                                    <i class=""fa-solid fa-pen""></i>
                                </a>
                            </td>
                            <td>{item.TripName}</td>
                            <td>{item.TripBranchSeq}</td>   
                            <td>{item.Tag}</td>   
                            <td class=""trip-record-id"" hidden>{item.TripRecordID}</td>
                            <td hidden>{notHasTripName}</td>
                            <td>{item.IdentifyNumber}</td>
                            <td>{item.ArrivedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                            <td>{departed}</td>
                            <td>{item.ArrivalLoadStatus}</td>
                            <td>{item.DepartureLoadStatus}</td>
                            <td>
                                <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                    onclick=""OnArrivalLoadImageClick('{item.TripRecordID}', this, 'EditLoadRecord')"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                    <i class=""fa-solid fa-truck""></i>
                                </a>
                            </td>
                            <td>
                                <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                    onclick=""OnDepartureLoadImageClick('{item.TripRecordID}', this, 'EditLoadRecord')"" data-id=""{item.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                    <i class=""fa-solid fa-truck""></i>
                                </a>
                            </td>
                            <td>{item.DriverName}</td>
                            <td>{item.StationName}</td>
                            <td>{truckNumber}</td>
                            <td>{item.DepoName}</td>
                            <td>{arrivalScheduledTime}</td>
                            <td>{departureScheduledTime}</td>
                            <td>{item.WorkDay.ToString("yyyy/MM/dd")}</td>
                            <td>{unlinkedReason}</td>
                        </tr>
                ";
                }
            }
            searchData += $@"
                        </tbody>
                    </table>
                </div>
            ";

            var searchedTripRecordListModel = new SearchedTripRecordListModel()
            {
                searchedTripRecordHTML = searchData,
                searchedTripRecordLength = tripRecordList.Count()
            };

            return Json(searchedTripRecordListModel);
        }

        /// <summary>
        /// 便実績修正登録画面表示
        /// </summary>
        /// <param name="tripRecordID"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register(int tripRecordID)
        {
            var model = new EditLoadRecordModel();
            try
            {
                var tripRecordSql = EditLoadRecordConnectController.CreateSQLToSelectTripRecordFromID(tripRecordID);
                model = ConnectToSQLServer.ExecuteQueryToList<EditLoadRecordModel>(tripRecordSql).First();
                model.RegistArrivalScheduledTime = ConversionScheduledTime(model.ArrivalScheduledTime);
                model.RegistDepartureScheduledTime = ConversionScheduledTime(model.DepartureScheduledTime);

                var identifyNumberSql = EditLoadRecordConnectController.CreateSQLToSelectIdentifyNumbers();
                var identifyNumberList = ConnectToSQLServer.ExecuteQueryToList<string>(identifyNumberSql);

                var unlinkedReasonSql = EditLoadRecordConnectController.CreateSQLToSelectUnlinkedReasons();
                var unlinkedReasonList = ConnectToSQLServer.ExecuteQueryToList<UnlinkedReasonModel>(unlinkedReasonSql);

                // 識別番号のセレクトリスト作成
                SelectListItem firstItem = new()
                {
                    Text = "選択してください",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                };
                model.IdentifyNumberSelectList.Add(firstItem);
                model.UnLinkedReasonSelectList.Add(firstItem);

                foreach (var identifyNumber in identifyNumberList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = Convert.ToString(identifyNumber),
                        Value = Convert.ToString(identifyNumber),
                        Selected = false
                    };

                    model.IdentifyNumberSelectList.Add(menuItem);
                }

                foreach (var unlinkedReason in unlinkedReasonList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = unlinkedReason.UnlinkedReasonName,
                        Value = unlinkedReason.UnlinkedReasonID.ToString(),
                        Selected = false
                    };

                    model.UnLinkedReasonSelectList.Add(menuItem);
                }

                model.ArrivalLoadImgPath = LoadRecordController.CheckAndConvertImagePath(model.ArrivalLoadImgPath);
                model.DepartureLoadImgPath = LoadRecordController.CheckAndConvertImagePath(model.DepartureLoadImgPath);

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(model);
            }
        }

        private string ConversionScheduledTime(DateTime? scheduledTime)
        {
            if (scheduledTime != null)
                return scheduledTime.Value.ToString("HH:mm");
            else
                return "-";
        }

        /// <summary>
        /// 便枝番リスト取得
        /// </summary>
        /// <param name="identifyNumber">識別番号</param>
        /// <param name="arrivedAt">到着実績</param>
        /// <returns></returns>
        public List<SelectListItem> GetTripBranchSeqs(string identifyNumber, DateTime arrivedAt)
        {
            var tripBranchNumberItems = new List<SelectListItem>();
            try
            {
                var sql = EditLoadRecordConnectController.CreateSQLToSelectTripBranchSeqsFromIdentifyNumber(identifyNumber, arrivedAt);
                var tripBranchNumbers = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(sql);
                tripBranchNumbers = M_TripBranchNumberController.AddTripBranchSeq(tripBranchNumbers);
                var sortedTripBranchNumbers = tripBranchNumbers.OrderBy(x => x.TripBranchSeq);
                // 便枝番のセレクトリスト作成
                SelectListItem firstItem = new()
                {
                    Text = "選択してください",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                };
                tripBranchNumberItems.Add(firstItem);

                foreach (var tripBranchNumber in sortedTripBranchNumbers)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = Convert.ToString(tripBranchNumber.TripBranchSeq),
                        Value = Convert.ToString(tripBranchNumber.TripBranchNumberID),
                        Selected = false
                    };

                   tripBranchNumberItems.Add(menuItem);
                }
                return tripBranchNumberItems;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripBranchNumberItems;
            }
        }

        /// <summary>
        /// 便名称取得
        /// </summary>
        /// <param name="identifyNumber"></param>
        /// <param name="arrivedAt"></param>
        /// <returns></returns>
        public M_TripModel GetTripModel(string identifyNumber, DateTime arrivedAt)
        {
            var tripModel = new M_TripModel();
            try
            {
                var sql = EditLoadRecordConnectController.CreateSQLToSelectTripNameFromIdentifyNumber(identifyNumber, arrivedAt);
                tripModel = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(tripModel.TripName))
                    tripModel.TripName = "便マスター未登録番号";
                return tripModel;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return tripModel;
            }
        }

        /// <summary>
        /// 出発・到着予定時間取得
        /// </summary>
        /// <param name="tripBranchNumberID"></param>
        /// <returns></returns>
        public M_TripBranchNumberModel GetScheduledTime(int tripBranchNumberID)
        {
            var schedules = new M_TripBranchNumberModel();
            try
            {
                var sql = EditLoadRecordConnectController.CreateSQLToSelectScheduledTimeFromTripBranchNumberID(tripBranchNumberID);
                schedules = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(sql).First();
                return schedules;
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return schedules;
            }
        }

        /// <summary>
        /// 便実績登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(EditLoadRecordModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                //入力規則チェック
                if (!ModelState.IsValid || model.TripBranchNumberID == 0)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"便実績更新失敗 {errorMessage}");

                    return NotFound(new { errorMessage });
                }

                // 変更項目モデル作成
                var correctionItems = GetCorrectionItems(model, user.UserName);
                var correctionItemSql = EditLoadRecordConnectController.CreateSQLToInsertCorrectionItems(correctionItems, DateTime.Now, user.UserName);
                ConnectToSQLServer.ExecuteQuery(correctionItemSql);

                // 便実績更新
                var sql = EditLoadRecordConnectController.CreateSQLToUpdateTripRecord(model);
                ConnectToSQLServer.ExecuteQuery(sql);

                // log取得
                _logger.Info($"便実績更新成功 便実績ID:{model.TripRecordID}");
                
                return Ok();
            }
            catch (SqlException ex)
            {
                // log取得
                errorMessage = "E3004: " + ErrorMessagesResources.E3004;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
            catch (Exception ex)
            {
                // log取得
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                var exceptionMessage = ex.Message;
                _logger.Error($"{exceptionMessage} {errorMessage}");

                return NotFound(new { errorMessage });
            }
        }

        private CorrectionItemModel GetCorrectionItems(EditLoadRecordModel model, string userName)
        {
            CorrectionItemModel result = new CorrectionItemModel();
            var sql = EditLoadRecordConnectController.CreateSQLToSelectTripRecordFromID(model.TripRecordID);
            var beforeModel = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql).First();

            result.TripRecordID = model.TripRecordID;

            // 各項目の変更有無設定
            if(beforeModel.TripID != model.TripID)
                result.IsTripIDChanged = true;
            else
                result.IsTripIDChanged = false;

            if(beforeModel.IdentifyNumber != model.IdentifyNumber)
                result.IsIdentifyNumberChanged = true;
            else
                result.IsIdentifyNumberChanged = false;

            if(beforeModel.TripBranchNumberID != model.TripBranchNumberID)
                result.IsTripBranchNumberIDChanged = true;
            else
                result.IsTripBranchNumberIDChanged = false;

            if(beforeModel.ArrivedAt != model.ArrivedAt)
                result.IsArrivedAtChanged = true;
            else
                result.IsArrivedAtChanged = false;

            if(beforeModel.DepartedAt != model.DepartedAt)
                result.IsDepartedAtChanged = true;
            else
                result.IsDepartedAtChanged= false;

            if(beforeModel.ArrivalLoadImgPath != model.ArrivalLoadImgPath)
                result.IsArrivalLoadImgPathChanged = true;
            else
                result.IsArrivalLoadImgPathChanged= false;

            if (beforeModel.DepartureLoadImgPath != model.DepartureLoadImgPath)
                result.IsDepartureLoadImgPathChanged = true;
            else
                result.IsDepartureLoadImgPathChanged = false;

            // 変更前の各項目の値保存
            result.BeforeTripID = beforeModel.TripID;
            result.BeforeIdentifyNumber = beforeModel.IdentifyNumber;
            result.BeforeTripBranchNumberID = beforeModel.TripBranchNumberID;
            result.BeforeArrivedAt = beforeModel.ArrivedAt;
            result.BeforeDepartedAt = beforeModel.DepartedAt;
            result.BeforeArrivalLoadImgPath = beforeModel.ArrivalLoadImgPath;
            result.BeforeDepartureLoadImgPath = beforeModel.DepartureLoadImgPath;

            if (beforeModel.RevisionArrivalLoadClass != model.RevisionArrivalLoadClass)
                LoadRecordController.InsertOrUpdateAnnotationLoads2(model.TripRecordID, model.RevisionArrivalLoadClass, userName, true);

            if (beforeModel.RevisionDepartureLoadClass != model.RevisionDepartureLoadClass)
                LoadRecordController.InsertOrUpdateAnnotationLoads2(model.TripRecordID, model.RevisionDepartureLoadClass, userName, false);
            return result;
        }
    }
}
