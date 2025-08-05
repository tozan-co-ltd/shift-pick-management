using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadProgressController : BaseController
    {
        public IActionResult Index()
        {
            var user = ClaimsLoginUserData();
            LoadProgressModel model = GetLoadProgress(user.MainDepoID, DateTime.Now);
            model.SelectedDepo = new M_DepoModel { 
                DepoID = user.MainDepoID,
                Name = user.MainDepoName
            };
            return View(model);
        }

        // 進捗表作成
        public LoadProgressModel GetLoadProgress(int depoId, DateTime workDay)
        {
            var model = new LoadProgressModel();

            var trips = GetTrips(depoId);

            // 便枝番情報取得
            var tripBranchNumbers = GetTripBranchNumbers(depoId, workDay);

            // 便実績情報取得
            var loadRecords = GetLoadRecords(depoId, workDay);

            // 便実績紐づけチェック
            model.LoadRecords = CheckLoadRecords(loadRecords, trips);
            model.TripBranchNumbers = tripBranchNumbers;

            return model;
        }

        /// <summary>
        /// トラックヤード指定時間のJsonリスト取得
        /// </summary>
        /// <returns>strList</returns>
        public IActionResult GetLoadProgressJson(int depoId)
        {
            try
            {
                // 便一覧を取得
                var trips = GetTrips(depoId);
                var tripBranchNumbers = GetTripBranchNumbers(depoId, DateTime.Now);
                // 便実績一覧を取得
                var loadRecords = GetLoadRecords(depoId, DateTime.Now);

                if (tripBranchNumbers.Count > 0)
                {
                    Dictionary<string, List<object>> dictData = new();
                    List<object> listBreak = new();
                    List<object> Schedule;

                    // 使用する便名
                    var lstTripName = GetTripNames(trips);

                    tripBranchNumbers.ForEach(lst =>
                    {
                        // トラックヤード名の存在をチェック
                        if (lstTripName.Contains(lst.TripName))
                        {
                            if (!dictData.ContainsKey(lst.TripName))
                            {
                                var Schedule = new List<object>();

                                // 辞書に追加
                                dictData.Add(lst.TripName, Schedule);
                            }

                            Schedule = dictData[lst.TripName];

                            // 昼勤開始時間と積込開始・終了予定時間を比較し、積込日を補正する
                            // マイナスの場合は、積込日+1
                            // ex. 積込日=2023/1/1,開始休憩時間=05:00,
                            // 積込開始=23:39:00,積込終了=0:09:00の場合、積込開始=2023/1/1,積込終了=2023/1/2となる
                            var startTime = TimeSpan.Parse(lst.ArrivalScheduledTime.ToString("HH:mm"));
                            var endTime = TimeSpan.Parse(lst.DepartureScheduledTime.ToString("HH:mm"));
                            var loadDate = DateTime.Now;
                            var fromDateTime = "";
                            var toDateTime = "";
                            var dayShiftStartTime = TimeSpan.Parse(lst.DayShiftStartTime.ToString("HH:MM"));

                            // 積込開始予定時間
                            if (startTime < dayShiftStartTime)
                                // 積込日+1
                                loadDate = loadDate.AddDays(1);

                            fromDateTime = loadDate.ToString("yyyy/MM/dd") + " " + startTime.ToString();

                            // 積込終了予定時間
                            if ((startTime < dayShiftStartTime && endTime > dayShiftStartTime) ||
                                (startTime > dayShiftStartTime && endTime < dayShiftStartTime))
                                // 積込日+1
                                loadDate = loadDate.AddDays(1);

                            toDateTime = loadDate.ToString("yyyy/MM/dd") + " " + endTime.ToString();

                            Schedule.Add(new
                            {
                                routeName = lst.TripName,
                                routeSeq = lst.TripBranchSeq,
                                from = fromDateTime,
                                to = toDateTime,
                                shipping_start_scheduled_time = loadDate + " " + lst.ArrivalScheduledTime.ToString("HH:mm"),
                                shipping_end_scheduled_time = loadDate + " " + lst.DepartureScheduledTime.ToString("HH:mm")
                            });
                        }
                    });

                    // 現在時刻を辞書に追加
                    var dictDataFormat = AddDateTimeNowToDictionary(dictData);

                    // 辞書データをソート
                    var sortedDictionary = dictDataFormat.OrderBy(x => x.Key, new SpecialKeyComparer()).ToDictionary(x => x.Key, x => x.Value);

                    var result = new { res = "OK", data = sortedDictionary.ToArray() };

                    return Json(result);
                }
                else
                {
                    var result = new { res = "NG", error = "E2007 該当データがありません。" };
                    return Json(result);
                }
            }
            catch (Exception)
            {
                var result = new { res = "NG", error = "E9999 予期せぬエラーが発⽣しました。" };
                return Json(result);
            }
        }

        /// <summary>
        /// 現在時刻を辞書に追加
        /// </summary>
        /// <returns>dictDataFormat</returns>
        public Dictionary<string, List<object>> AddDateTimeNowToDictionary(Dictionary<string, List<object>> dictData)
        {
            Dictionary<string, List<object>> dictDataFormat = new();

            foreach (var item in dictData)
            {
                var selected = new List<object>
                {
                    new
                    {
                        from = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        to = DateTime.Now.AddMinutes(5).ToString("yyyy/MM/dd HH:mm:ss"),
                        shipping_lane_status_name = "現在時刻"
                    }
                };
                var itemFormat = item.Value.Except(selected).ToList();
                itemFormat.AddRange(selected);

                // 辞書の最後尾に追加
                dictDataFormat.Add(item.Key, itemFormat);
            }

            return dictDataFormat;
        }

        public List<string> GetTripNames(List<M_TripModel> trips)
        {
            List<string> tripNames = new ();
            foreach (var trip in trips)
            {
                tripNames.Add(trip.TripName);
            }
            return tripNames;
        }

        // 便情報取得
        public static List<M_TripModel> GetTrips(int depoId)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectM_TripsFromDepo(depoId);
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
            return trips;
        }

        // 便枝番情報取得
        public static List<M_TripBranchNumberModel> GetTripBranchNumbers(int depoId, DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectTripBranchNumbersFromDepo(depoId, workDay);
            var tripBranchNumbers = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(sql);
            return tripBranchNumbers;
        }

        // 便実績情報取得
        public static List<LoadRecordModel> GetLoadRecords(int depoId, DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectLoadRecordsFromDepoAndWorkDay(depoId, workDay);
            var loadRecords = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
            return loadRecords;
        }

        // 便実績紐づけチェック
        public List<LoadRecordModel> CheckLoadRecords(List<LoadRecordModel> loadRecords, List<M_TripModel> trips)
        {
            foreach (var loadRecord in loadRecords)
            {
                // 便名称が
                if (string.IsNullOrEmpty(loadRecord.TripName) && !string.IsNullOrEmpty(loadRecord.IdentifyNumber))
                {
                    var trip = trips.Find(x => x.IdentifyNumber == loadRecord.IdentifyNumber);
                    if (trip != null)
                        loadRecord.TripName = trip.TripName;
                }
            }
            return loadRecords;
        }

        /// <summary>
        /// 辞書データをソート
        /// </summary>
        /// <returns>x.CompareTo(y)</returns>
        class SpecialKeyComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var sql = LoadProgressConnectController.CreateSQLToSelectTripNames();
                var tripNames = ConnectToSQLServer.ExecuteQueryToList<string>(sql);
                for (var i = 0; i < tripNames.Count; i++)
                {
                    if (x == tripNames[i] && y != tripNames[i])
                        return -(i + 1);
                    if (x != tripNames[i] && y == tripNames[i])
                        return i + 1;
                }

                return x.CompareTo(y);
            }
        }
    }
}
