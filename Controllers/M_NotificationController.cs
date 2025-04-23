using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
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
                var mainDepo = GetMainDepo();
                model.MainDepo = mainDepo;
                List<string> depoList = new();
                depoList.Add(mainDepo.DepoID.ToString());

                IEnumerable<M_NotificationModel> notificationList = new List<M_NotificationModel>();

                if (depoList.Count > 0)
                {
                    // 便マスター情報取得SQL作成
                    var sql = M_NotificationConnectController.CreateSQLToSelectMNotifications(true, depoList);
                    // DB接続
                    notificationList = M_NotificationConnectController.ConnectMNotifications(sql);
                    // 荷量クラスを％表示に変換
                    foreach(M_NotificationModel notification in notificationList)
                    {
                        notification.ArrivalLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(notification.ArrivalLowerLoadClass) + " 未満";
                        notification.DepartureLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(notification.DepartureLowerLoadClass) + " 未満";
                    }
                }

                model.M_NotificationList = notificationList.ToList();

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
                    notificationList = M_NotificationConnectController.ConnectMNotifications(sql);
                    // 荷量クラスを％表示に変換
                    foreach (M_NotificationModel notification in notificationList)
                    {
                        notification.ArrivalLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(notification.ArrivalLowerLoadClass) + " 未満";
                        notification.DepartureLowerLoadStatus = LoadRecordController.ConversionLoadClassToLoadStatus(notification.DepartureLowerLoadClass) + " 未満";
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
                M_NotificationConnectController.InsertMNotification(model, user);

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
            var tripList = M_NotificationConnectController.ConnectMTrips(tripSql);
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
            var tripBranchSeqList = M_NotificationConnectController.ConnectMTripBranchNumbers(tripBranchSeqSql);
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
            var tripBranchSeqList = M_NotificationConnectController.ConnectMTripBranchNumbers(tripBranchSeqSql);
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
            var sql = M_UserConnectController.CreateSQLToSelectMUsers();
            var userList = M_UserConnectController.ConnectMUsers(sql);
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
                                    <input type=""checkbox"" name=""adName"" id=""{user.ADName}"" value=""{user.ADName}/{user.UserID}"">{user.ADName}
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
    }
}
