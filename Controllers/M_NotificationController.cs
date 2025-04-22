using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ai_truck_load_measurement.Controllers
{
    public class M_NotificationController : BaseController
    {
        public IActionResult Index()
        {
            var model = new M_NotificationViewModel();
            model.MainDepo = GetMainDepo();
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new M_NotificationRegisterViewModel();
            model.TripNameSelectList = GetTrips();
            
            return View(model);
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
                    Value = Convert.ToString(tripBranch.TripBranchNumberID),
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
            var countBeforeShiftStartTimeRow = 0; // 到着予定時間が昼勤開始時間より早い行の数
            var tripBranchSeq = 1; // 枝連番

            for (int i = 0; i < tripBranchNumberList.Count; i++)
            {

                var tripBranchNumber = tripBranchNumberList[i];
                var dayShiftStartTime = tripBranchNumber.DayShiftStartTime;
                var arrivalScheduledTime = tripBranchNumber.ArrivalScheduledTime;

                // 到着予定時間が昼勤開始時間以降のデータの場合、枝連番付与
                // それ以外の場合、昼勤開始時間以前の行数のカウントを1増やす
                if (arrivalScheduledTime > dayShiftStartTime)
                {
                    tripBranchNumber.TripBranchSeq = tripBranchSeq;
                    tripBranchSeq++;
                }
                else
                {
                    countBeforeShiftStartTimeRow++;
                }
            }

            // 到着予定時間が昼勤開始時間以前のデータに枝連番付与
            if (countBeforeShiftStartTimeRow > 0)
            {
                for (int i = 0; i < countBeforeShiftStartTimeRow; i++)
                {
                    var tripBranchNumber = tripBranchNumberList[i];
                    tripBranchNumber.TripBranchSeq = tripBranchSeq;
                    tripBranchSeq++;
                }
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
