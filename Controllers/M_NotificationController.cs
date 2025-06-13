using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Data;
using X.PagedList;

namespace ai_truck_load_measurement.Controllers
{
    public class M_NotificationController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            var model = new M_NotificationViewModel();
            try
            {
                // ログインユーザーのメインデポ情報取得
                var user = ClaimsLoginUserData();
                model.MainDepoID = user.MainDepoID;
                model.MainDepoName = user.MainDepoName;

                IEnumerable<M_NotificationModel> notificationList = new List<M_NotificationModel>();

                // 通知マスター情報取得SQL作成
                var notificationSql = M_NotificationConnectController.CreateSQLToSelectMNotificationsAll();
                // DB接続
                notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(notificationSql);
                foreach (M_NotificationModel notification in notificationList)
                {
                    // 通知ユーザー情報取得SQL作成
                    var notificationUserSql = M_NotificationConnectController.CreateSQLToSelectRNotificationUsers(notification.NotificationID);
                    notification.NotificationUsers = ConnectToSQLServer.ExecuteQueryToList<R_NotificationUserModel>(notificationUserSql);

                }

                model.M_NotificationList = notificationList.ToList();
                model.DepoSelectList = GetDepos();
                model.TripNameSelectList = GetTrips();

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
        /// 便情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public IActionResult SearchData(bool isBeforeNotificationPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<M_NotificationModel> notificationList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    // 便マスター情報取得SQL作成
                    var sql = M_NotificationConnectController.CreateSQLToSelectMNotifications(isBeforeNotificationPeriod, checkedDepos);
                    // DB接続
                    notificationList = ConnectToSQLServer.ExecuteQueryToList<M_NotificationModel>(sql);
                    foreach (M_NotificationModel notification in notificationList)
                    {
                        // 通知ユーザー情報取得SQL作成
                        var notificationUserSql = M_NotificationConnectController.CreateSQLToSelectRNotificationUsers(notification.NotificationID);
                        notification.NotificationUsers = ConnectToSQLServer.ExecuteQueryToList<R_NotificationUserModel>(notificationUserSql);

                        // 荷量クラスを％表示に変換
                        notification.ArrivalLowerLoadStatus = ConversionLoadClassToLoadStatus(notification.ArrivalLowerLoadClass);
                        notification.DepartureLowerLoadStatus = ConversionLoadClassToLoadStatus(notification.DepartureLowerLoadClass);
                    }
                }

                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""notificationTable"">
                            <thead>
                                <tr align=""center"">
                                    <th width=""40""></th>
                                    <th class=""font-weight-bold"">通知ID</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">便枝番</th>
                                    <th class=""font-weight-bold"">デポ名</th>
                                    <th class=""font-weight-bold"">到着荷量下限</th>
                                    <th class=""font-weight-bold"">出発荷量下限</th>
                                    <th class=""font-weight-bold"">通知開始日時</th>
                                    <th class=""font-weight-bold"">通知終了日時</th>
                                    <th class=""font-weight-bold"">メール受信者</th>
                                    <th class=""font-weight-bold"">更新日時</th>
                                    <th class=""font-weight-bold"">更新者</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // 新しい便情報テーブルのhtml作成
                if (notificationList.Count > 0)
                {
                    foreach (var item in notificationList)
                    {
                        var notificationUserHtml = GetNotificationUserHTML(item);
                        searchData += $@"
                            <tr>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                       onclick=""OnEditClick('{@item.NotificationID}')"" data-id=""{item.NotificationID}"" data-toggle=""modal"" data-target=""#edit-modal"">
                                        <i class=""fa-solid fa-pen""></i>
                                    </a>
                                    <button class=""btn btn-danger btn-icon-split""
                                                onclick=""OnDeleteClick('{@item.NotificationID}')"" data-id=""{@item.NotificationID}"" data-toggle=""modal"" data-target=""#delete-modal"">
                                            <i class=""fa-solid fa-trash""></i>
                                    </button>
                                </td>
                                <td>{item.NotificationID}</td>
                                <td>{@item.TripName}</td>
                                <td>{@item.TripBranchSeq}</td>
                                <td>{@item.DepoName}</td>
                                <td>{@item.ArrivalLowerLoadStatus}</td>
                                <td>{@item.DepartureLowerLoadStatus}</td>
                                <td>{@item.NotificationStartDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.NotificationEndDateTime.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{notificationUserHtml}</td>
                                <td>{@item.UpdatedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{@item.UpdatedBy}</td>
                            </tr>
                    ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

                return Content(searchData);
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Content(errorMessage);
            }
        }

        /// <summary>
        /// 通知先ユーザーリストのHTML作成
        /// </summary>
        /// <param name="model">通知モデル</param>
        /// <returns></returns>
        private string GetNotificationUserHTML(M_NotificationModel model)
        {
            var html = "";
            // 通知先が0の場合は空でリターン
            if (model.NotificationUsers == null)
                return html;

            // 通知先リストをhtml化
            for(int i = 0; i < model.NotificationUsers.Count; i++) { 
                if(i != 0)
                {
                    html += "<br />";
                }
                html += model.NotificationUsers[i].ADName;
            }
            return html;
        }

        /// <summary>
        /// 登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            var model = new M_NotificationRegisterViewModel();
            model.DepoSelectList = GetDepos();
            model.TripNameSelectList = GetTrips();
            
            return View(model);
        }

        /// <summary>
        /// 便マスター登録
        /// </summary>
        /// <param name="model">登録情報</param>
        [HttpPost]
        public IActionResult Register(M_NotificationModel model)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                model = ConvertTripBranchSeqFromTripBranchIDAndSeq(model);

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");
                    var errormsgs = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage });
                }


                // 便マスター登録
                M_NotificationConnectController.InsertMNotificationAndRNotificationUser(model, user);

                // log取得
                _logger.Info($"便マスター登録成功 便名称:{model.TripName}");
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

        /// <summary>
        /// 便マスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        [HttpPost]
        public IActionResult Edit(M_NotificationModel model)
        {
            string? errorMessage;
            try
            {

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                //model = ConvertTripBranchSeqFromTripBranchIDAndSeq(model);

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    // log取得
                    errorMessage = "E1011: " + ErrorMessagesResources.E1011;
                    _logger.Error($"便マスター登録失敗 {errorMessage}");
                    var errormsgs = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage });
                }


                // 便マスター登録
                M_NotificationConnectController.UpdateMNotificationAndRNotificationUser(model, user);

                // log取得
                _logger.Info($"便マスター登録成功 便名称:{model.TripName}");
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

        /// <summary>
        /// 「便枝番ID_枝連番」から枝連番を取得、保存する
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private M_NotificationModel ConvertTripBranchSeqFromTripBranchIDAndSeq(M_NotificationModel model)
        {
            var tripBranchIDAndSeq = model.TripBranchIDAndSeq.Split('_');
            model.TripBranchSeq = Int32.Parse(tripBranchIDAndSeq[1]);
            return model;
        }

        /// <summary>
        /// 車両マスター削除
        /// </summary>
        /// <param name="truckId">車両ID</param>
        /// <returns></returns>
        public IActionResult Delete(int notificationID)
        {
            string? errorMessage;
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 車両マスター削除
                int deleteAffectedRows = M_NotificationConnectController.DeleteMNotificationAndRNotificationUser(notificationID, user);

                // log取得
                _logger.Info($"通知マスター削除成功 通知ID:{notificationID}");

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


        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, bool isBeforeNotificationPeriod, List<string> checkedDepos)
        {
            string? errorMessage;
            try
            {
                DataTable mNotificationDT = new DataTable(); // 通知マスター用データテーブル
                DataTable rNotificationUserDT = new DataTable(); // 通知ユーザー用データテーブル
                DataTable mNotificationConvertedDT = new DataTable();

                // デポが選択されていない場合
                if (checkedDepos.Count == 0)
                {
                    // エラーメッセージ取得
                    // 「ファイルが存在しません。」
                    errorMessage = ErrorMessagesResources.E9999;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // 通知マスター情報取得
                var mNotificationSql = M_NotificationConnectController.CreateSQLToSelectMNotificationsForDataTable(isBeforeNotificationPeriod, checkedDepos);
                mNotificationDT = ConnectToSQLServer.ConnectToDataTable(mNotificationSql);
                // 荷量クラスを数値に変換
                mNotificationConvertedDT = GetConvertedLoadClassDataTable(mNotificationDT);

                // 通知ユーザー情報取得
                var rNotificationUserSql = M_NotificationConnectController.CreateSQLToSelectRNotificationUsersForDataTable(isBeforeNotificationPeriod, checkedDepos);
                rNotificationUserDT = ConnectToSQLServer.ConnectToDataTable(rNotificationUserSql);

                // ファイル名
                var tmpFilename = CreateFile.CreateFileName(gamenName);
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "通知マスター";
                string sheetNameTwo = "通知ユーザー";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(mNotificationConvertedDT, rNotificationUserDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);

                        CreateFile.DeleteFile(tmpFilename);

                        return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
                    }
                    else
                    {
                        // エラーメッセージ取得
                        // 「ファイルが存在しません。」
                        errorMessage = ErrorMessagesResources.E9999;

                        return Json(new { res = "NG", error = errorMessage });
                    }
                }
                catch (Exception ex)
                {
                    // エラーメッセージ取得
                    // 「NASに接続できませんでした。」
                    errorMessage = ErrorMessagesResources.E9999;

                    // log取得
                    var exceptionMessage = ex.Message;
                    return Json(new { res = "NG", error = errorMessage + exceptionMessage });
                }
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;

                // log取得
                var exceptionMessage = ex.Message;
                return Json(new { res = "NG", error = errorMessage + exceptionMessage });
            }

        }

        /// <summary>
        /// データテーブルの荷量クラスを数値に変換
        /// </summary>
        /// <param name="dt">変換元データテーブル</param>
        /// <returns></returns>
        public  DataTable GetConvertedLoadClassDataTable(DataTable dt)
        {
            // テーブルに値を変換した後の文字列を格納する列を追加
            dt.Columns.Add("arrival_lower_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("arrival_lower_load_class"));
            dt.Columns.Add("departure_lower_load_status", typeof(string)).SetOrdinal(dt.Columns.IndexOf("departure_lower_load_class"));

            // 各列の値を適切な値に変換
            foreach (DataRow row in dt.Rows)
            {
                // 荷量クラスを%表示に変換
                var arrivalLoadClass = (int)row["arrival_lower_load_class"];
                var departureLoadClass = (int)row["departure_lower_load_class"];
                row["arrival_lower_load_status"] = ConversionLoadClassToLoadStatus(arrivalLoadClass);
                row["departure_lower_load_status"] = ConversionLoadClassToLoadStatus(departureLoadClass);
            }

            // 変換前の列を削除
            dt.Columns.Remove("arrival_lower_load_class");
            dt.Columns.Remove("departure_lower_load_class");
            return dt;
        }

        /// <summary>
        /// デポリスト取得
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetDepos()
        {

            var selectList = new List<SelectListItem>();
            // 1行目作成
            selectList.Add(new SelectListItem
            {
                Text = "選択してください",
                Value = "0",
                Selected = true,
                Disabled = true,
            });

            // 便の選択肢作成
            var depoSql = M_NotificationConnectController.CreateSQLToSelectMDepos();
            var depoList = ConnectToSQLServer.ExecuteQueryToList<M_DepoModel>(depoSql);
            foreach (var depo in depoList)
            {
                SelectListItem menuItem = new()
                {
                    Text = Convert.ToString(depo.Name),
                    Value = Convert.ToString(depo.DepoID),
                    Selected = false
                };
                selectList.Add(menuItem);
            }
            return selectList;
        }


        /// <summary>
        /// 便名称リスト取得
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetTrips()
        {

            var selectList = new List<SelectListItem>();
            // 1行目作成
            selectList.Add(new SelectListItem
            {
                Text = "選択してください",
                Value = "0",
                Selected = true,
                Disabled = true,
            });

            // 便の選択肢作成
            var tripSql = M_NotificationConnectController.CreateSQLToSelectMTrips();
            var tripList = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(tripSql);
            foreach (var trip in tripList)
            {
                SelectListItem menuItem = new()
                {
                    Text = Convert.ToString(trip.TripName),
                    Value = Convert.ToString(trip.TripID),
                    Selected = false
                };
                selectList.Add(menuItem);
            }
            return selectList;
        }

        /// <summary>
        /// 便枝番リスト取得
        /// </summary>
        /// <param name="tripID">便ID</param>
        /// <returns></returns>
        public List<SelectListItem> GetTripBranchSeqs(int tripID)
        {
            var selectList = new List<SelectListItem>();
            // 1行目作成
            selectList.Add(new SelectListItem
            {
                Text = "選択してください",
                Value = "0",
                Selected = true,
                Disabled = true,
            });

            var tripBranchSeqSql = M_NotificationConnectController.CreateSQLToSelectMTripBranchNumbers(tripID);
            var tripBranchSeqList = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(tripBranchSeqSql);
            // 枝連番行を追加
            tripBranchSeqList = AddTripBranchSeq(tripBranchSeqList);
            foreach(var tripBranch in tripBranchSeqList)
            {
                SelectListItem menuItem = new()
                {
                    Text = Convert.ToString(tripBranch.TripBranchSeq),
                    Value = Convert.ToString(tripBranch.TripBranchNumberID) + "_" + Convert.ToString(tripBranch.TripBranchSeq),
                    Selected = false
                };
                selectList.Add(menuItem);
            }

            return selectList;
        }

        /// <summary>
        /// 便枝番リストに枝連番を追加する
        /// </summary>
        /// <param name="tripBranchNumberList">枝連番を追加する対象便枝番リスト</param>
        /// <returns></returns>
        private List<M_TripBranchNumberModel> AddTripBranchSeq(List<M_TripBranchNumberModel> tripBranchNumberList)
        {
            var tripBranchSeq = 1; // 枝連番

            foreach (var tripBranchNumber in tripBranchNumberList) 
            {
                tripBranchNumber.TripBranchSeq = tripBranchSeq;
                tripBranchSeq++;
            }

            return tripBranchNumberList;
        }

        /// <summary>
        /// 便名称と便枝番から便枝番の詳細情報を取得する
        /// </summary>
        /// <param name="tripID"></param>
        /// <param name="tripBranchNumberID"></param>
        /// <returns></returns>
        public M_TripBranchNumberModel GetTripBranchSeqData(int tripID, int tripBranchNumberID)
        {
            var tripBranchSeqSql = M_NotificationConnectController.CreateSQLToSelectMTripBranchNumberFromTripBranchNumberID(tripID, tripBranchNumberID);
            var tripBranchSeqList = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(tripBranchSeqSql);
            return tripBranchSeqList[0];
        }


        /// <summary>
        /// ユーザーセレクトリストのHTML取得
        /// </summary>
        /// <returns></returns>
        public string GetADNameHTML()
        {
            // ユーザーリスト取得
            var userList = GetUserList();
            // ユーザーリストからセレクトリストのHTML作成
            var html = CreateSelectADNameHTML(userList);
            return html;
        }

        /// <summary>
        /// ユーザーリスト取得
        /// </summary>
        /// <returns></returns>
        public List<M_UserModel> GetUserList()
        {
            var sql = M_UserConnectController.CreateSQLToSelectMUsersIsRequiredMail();
            var userList = ConnectToSQLServer.ExecuteQueryToList<M_UserModel>(sql);
            return userList;
        }

        /// <summary>
        /// ユーザーセレクトリストのHTML作成
        /// </summary>
        /// <param name="userList">ユーザーリスト</param>
        /// <returns></returns>
        private string CreateSelectADNameHTML(List<M_UserModel> userList)
        {
            var html = "";

            // ユーザーマスターにデータがない
            if (userList.Count == 0)
            {
                html = "<small>ユーザーマスターにユーザーデータがありません</small>";
                return html;
            }

            // ドロップダウンリストの最上部
            html += $@" 
                    <div class=""d-flex justify-content-between mb-1"">
                        <a href=""#"" class=""btn btn-secondary "" onclick=""allToggleOpen()"" >
                            <span class=""text"">全て展開</span>
                        </a>
                        <a href=""#"" class=""btn btn-secondary "" onclick=""allToggleClose()"" >
                            <span class=""text"">全て閉じる</span>
                        </a>
                    </div>
                    <hr />
            ";

            // ユーザーリストをデポIDごとにソート、グループ化
            var sortedList = userList.OrderBy(x => x.DepoID).
                GroupBy(x => x.DepoID);

            foreach (var userListInDepo in sortedList)
            {
                // デポ名表示HTML
                html += $@"
                        <div class=""medium-item"">
                            <div class=""medium-header"">
                                <div class=""toggle-icon collapsed""></div>
                                    <strong>　{userListInDepo.First().DepoName} </strong>
                            </div>
                            <div class=""small-items"">
                ";

                foreach (var user in userListInDepo)
                {
                        // ユーザーチェックボックスの追加
                        html += $@"
                                <label class=""checkbox-item"">
                                    <input type=""checkbox"" name=""NotificationUsersView"" id=""{user.ADName}"" value=""{user.ADName}/{user.UserID}"">{user.ADName}
                                </label>
                        ";
                }
                html += $@"
                            </div>
                        </div>
                ";
            }
            return html;
        }

        /// <summary>
        /// 荷量クラスからパーセント表示に変換
        /// </summary>
        /// <param name="loadClass">荷量クラス</param>
        /// <returns></returns>
        public static string ConversionLoadClassToLoadStatus(int loadClass)
        {
            var loadStatus = "-";
            if (loadClass >= 3)
            {
                int lowerLimit = (loadClass - 3) * 10 + 1;
                int upperLimit = (loadClass - 2) * 10;
                loadStatus = ($"{lowerLimit}-{upperLimit} 未満");
            }
            else if (loadClass == 2)
            {
                loadStatus = "設定無し";
            }
            return loadStatus;
        }
    }
}
