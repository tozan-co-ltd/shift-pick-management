using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

            var trips = GetTrips();

            // 便枝番情報取得
            var tripBranchNumbers = GetTripBranchNumbers(depoId, trips, workDay);

            // 便実績情報取得
            var loadRecords = GetLoadRecords(workDay);
            var loadRecordsConvertedStatus = LoadRecordController.ConversionForTable(loadRecords);

            // 便実績紐づけチェック
            model.LoadRecords = CheckLoadRecords(loadRecordsConvertedStatus.ToList(), trips);
            model.TripBranchNumbers = tripBranchNumbers;

            return model;
        }

        /// <summary>
        /// トラックヤード指定時間のJsonリスト取得
        /// </summary>
        /// <returns>strList</returns>
        public IActionResult GetLoadProgressJson(int depoId, DateTime loadDate)
        {

            try
            {
                // 便一覧を取得
                var trips = GetTripsFromDepo(depoId);
                // 便枝番一覧を取得
                var tripBranchNumbers = GetTripBranchNumbers(depoId, trips, loadDate);
                // 便実績一覧を取得
                var loadRecords = GetLoadRecordsFromDepo(depoId, loadDate);
                loadRecords = CheckLoadRecords(loadRecords, trips);

                if (tripBranchNumbers.Count > 0)
                {
                    Dictionary<string, List<object>> dictData = new();
                    List<object> listBreak = new();
                    List<object> Schedule;
                    List<object> AfterSchedule;

                    // 使用する便名
                    var lstTripName = GetTripNames(trips);

                    tripBranchNumbers.ForEach(lst =>
                    {
                        // 便名の存在をチェック
                        if (lstTripName.Contains(lst.TripName))
                        {
                            if (!dictData.ContainsKey(lst.TripName))
                            {
                                var Schedule = new List<object>();
                                var AfterSchedule = new List<object>(); // 便予定が日付をまたいだ時用
                                var LoadRecord = new List<object>();
                                var dayShiftStartTimeObject = new
                                {
                                    from = loadDate.ToString("yyyy/MM/dd") + " " + lst.DayShiftStartTime.ToString("HH:mm"),
                                    to = loadDate.ToString("yyyy/MM/dd") + " " + lst.DayShiftStartTime.AddMinutes(5).ToString("HH:mm"),
                                    trip_lane_status_name = "昼勤開始時間"
                                };

                                // 辞書に追加
                                dictData.Add(lst.TripName, Schedule);
                                dictData.Add(lst.TripName + "実績", LoadRecord);
                                if (lst.ArrivalScheduledTime > lst.DepartureScheduledTime)
                                    dictData.Add(lst.TripName, AfterSchedule);

                                // 昼勤開始時間を追加
                                Schedule.Add(dayShiftStartTimeObject);
                                LoadRecord.Add(dayShiftStartTimeObject);
                            }

                            Schedule = dictData[lst.TripName];
                            AfterSchedule = dictData[lst.TripName];

                            var startTime = TimeSpan.Parse(lst.ArrivalScheduledTime.ToString("HH:mm"));
                            var endTime = TimeSpan.Parse(lst.DepartureScheduledTime.ToString("HH:mm"));
                            var fromDateTime = "";
                            var toDateTime = "";
                            var afterFromDateTime = "";
                            var afterToDateTime = "";
                            var loadTime = TimeSpan.Parse(loadDate.ToString("HH:mm"));
                            var tripLaneStatusName = "";

                            // 便予定のステータスを決定
                            if (endTime < loadTime)
                                tripLaneStatusName = "出発後";
                            else if (loadTime < startTime)
                                tripLaneStatusName = "到着前";
                            else if (startTime <= loadTime && loadTime <= endTime)
                                tripLaneStatusName = "停車中";


                            if (startTime < endTime)
                            {
                                fromDateTime = loadDate.ToString("yyyy/MM/dd") + " " + startTime.ToString();
                                toDateTime = loadDate.ToString("yyyy/MM/dd") + " " + endTime.ToString();
                            }
                            else
                            {
                                // 便予定が日付をまたいでいる場合(ex. 到着予定: 23:00, 出発予定: 00:30)
                                fromDateTime = loadDate.ToString("yyyy/MM/dd") + " 00:00:00";
                                toDateTime = loadDate.ToString("yyyy/MM/dd") + " " + endTime.ToString();
                                afterFromDateTime = loadDate.ToString("yyyy/MM/dd") + " " + startTime.ToString();
                                afterToDateTime = loadDate.ToString("yyyy/MM/dd") + " 23:59:59";

                                AfterSchedule.Add(new
                                {
                                    routeName = lst.TripName,
                                    routeSeq = lst.TripBranchSeq,
                                    from = afterFromDateTime,
                                    to = afterToDateTime,
                                    arrival_scheduled_time = loadDate + " " + lst.ArrivalScheduledTime.ToString("HH:mm"),
                                    departure_scheduled_time = loadDate + " " + lst.DepartureScheduledTime.ToString("HH:mm"),
                                    trip_lane_status_name = tripLaneStatusName,
                                    schedule_or_record = "schedule",
                                    trip_id = lst.TripID
                                });

                            }

                            Schedule.Add(new
                            {
                                routeName = lst.TripName,
                                routeSeq = lst.TripBranchSeq,
                                from = fromDateTime,
                                to = toDateTime,
                                trip_lane_status_name = tripLaneStatusName,
                                schedule_or_record = "schedule",
                                trip_id = lst.TripID
                            });
                        }
                    });

                    if(loadRecords.Count > 0)
                    {
                        loadRecords.ForEach(lst =>
                        {

                            // 便名の存在をチェック
                            if (lstTripName.Contains(lst.TripName))
                            {
                                if (!dictData.ContainsKey(lst.TripName + "実績") )
                                {
                                    var Schedule = new List<object>();

                                    // 辞書に追加
                                    dictData.Add(lst.TripName + "実績", Schedule);
                                }
                                else if( dictData[lst.TripName + "実績"].Count == 0)
                                {
                                    var Schedule = new List<object>();
                                    dictData[lst.TripName + "実績"].Add(Schedule);
                                }

                                Schedule = dictData[lst.TripName + "実績"];

                                var startTime = TimeSpan.Parse(lst.ArrivedAt.ToString("HH:mm"));
                                var endTime = TimeSpan.Parse(lst.DepartedAt.ToString("HH:mm"));
                                // 出発データの有無判断
                                var isDeparted = true;
                                var hasDepartData = true;
                                if (lst.DepartedAt.ToString("yyyy/MM/dd") == "0001/01/01")
                                {
                                    hasDepartData = false;
                                    if(IsLatestRecordInSameStations(lst) && IsTruckExistedInSameStations(lst))
                                    {
                                        // 停車中の場合
                                        isDeparted = false;
                                        endTime = TimeSpan.Parse(loadDate.AddMinutes(5).ToString("HH:mm"));
                                    }
                                    else
                                    {
                                        // 出発済みかつ出発データが存在しない場合
                                        endTime = TimeSpan.Parse(lst.ArrivedAt.AddMinutes(15).ToString("HH:mm"));
                                    }
                                }
                                var fromDateTime = loadDate.ToString("yyyy/MM/dd") + " " + startTime.ToString();
                                var toDateTime = loadDate.ToString("yyyy/MM/dd") + " " + endTime.ToString();

                                var tripLaneStatus = "";
                                if (!string.IsNullOrEmpty(lst.TripBranchSeq))
                                    tripLaneStatus += "紐付け有";
                                else
                                    tripLaneStatus += "紐付け無";

                                if (isDeparted)
                                    tripLaneStatus += "出発済";
                                else
                                    tripLaneStatus += "停車中";

                                Schedule.Add(new
                                {
                                    routeName = lst.TripName,
                                    routeSeq = lst.TripBranchSeq,
                                    from = fromDateTime,
                                    to = toDateTime,
                                    trip_lane_status_name = tripLaneStatus,
                                    schedule_or_record = "record",
                                    trip_record_id = lst.TripRecordID,
                                    has_depart_data = hasDepartData
                                });
                            }
                        });
                    }

                    // 現在時刻を辞書に追加
                    var dictDataFormat = AddDateTimeToDictionary(dictData, loadDate, "現在時刻");

                    var result = new { res = "OK", data = dictDataFormat.ToArray() };

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
        public Dictionary<string, List<object>> AddDateTimeToDictionary(Dictionary<string, List<object>> dictData, DateTime targetTime, string statusName)
        {
            Dictionary<string, List<object>> dictDataFormat = new();

            foreach (var item in dictData)
            {
                var selected = new List<object>
                {
                    new
                    {
                        from = targetTime.ToString("yyyy/MM/dd HH:mm:ss"),
                        to = targetTime.AddMinutes(5).ToString("yyyy/MM/dd HH:mm:ss"),
                        trip_lane_status_name = statusName
                    }
                };
                var itemFormat = item.Value.Except(selected).ToList();
                itemFormat.AddRange(selected);

                // 辞書の最後尾に追加
                dictDataFormat.Add(item.Key, itemFormat);
            }

            return dictDataFormat;
        }

        /// <summary>
        /// 便リストから便名称のリストを取得
        /// </summary>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        public List<string> GetTripNames(List<M_TripModel> trips)
        {
            List<string> tripNames = new ();
            foreach (var trip in trips)
            {
                tripNames.Add(trip.TripName);
            }
            return tripNames;
        }

        /// <summary>
        /// 全便予定情報取得
        /// </summary>
        /// <returns></returns>
        public static List<M_TripModel> GetTrips()
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectM_Trips();
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
            return trips;
        }

        /// <summary>
        /// 指定したデポの便予定情報取得
        /// </summary>
        /// <param name="depoId">デポのID</param>
        /// <returns></returns>
        public static List<M_TripModel> GetTripsFromDepo(int depoId)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectM_TripsFromDepo(depoId);
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
            return trips;
        }

        /// <summary>
        /// 便枝番情報取得
        /// </summary>
        /// <param name="depoId">デポID</param>
        /// <param name="trips">便名</param>
        /// <param name="workDay">稼働日</param>
        /// <returns></returns>
        public List<M_TripBranchNumberModel> GetTripBranchNumbers(int depoId, List<M_TripModel> trips, DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectTripBranchNumbersFromDepo(depoId, workDay);
            var tripBranchNumbers = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(sql);
            tripBranchNumbers = SortTripBranchSeq(tripBranchNumbers, trips);
            return tripBranchNumbers;
        }

        /// <summary>
        /// 指定した稼働日の全便実績情報取得
        /// </summary>
        /// <param name="workDay">稼働日</param>
        /// <returns></returns>
        public List<LoadRecordModel> GetLoadRecords(DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectLoadRecordsFromWorkDay(workDay);
            var loadRecords = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
            return loadRecords;
        }

        /// <summary>
        /// 指定した稼働日、デポの便実績情報取得
        /// </summary>
        /// <param name="depoId">デポID</param>
        /// <param name="workDay">稼働日</param>
        /// <returns></returns>
        public static List<LoadRecordModel> GetLoadRecordsFromDepo(int depoId, DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectLoadRecordsFromDepoAndWorkDay(depoId, workDay);
            var loadRecords = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
            return loadRecords;
        }

        /// <summary>
        /// 便実績紐づけチェック
        /// </summary>
        /// <param name="loadRecords">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
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
        /// 便枝番リストのソート
        /// </summary>
        /// <param name="tripBranchNumberList">ソート対象の便枝番リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        public List<M_TripBranchNumberModel> SortTripBranchSeq(List<M_TripBranchNumberModel> tripBranchNumberList, List<M_TripModel> trips)
        {
            List<M_TripBranchNumberModel> results = new();
            // 便名称毎にグループ化
            foreach (M_TripModel trip in trips)
            {
                List<M_TripBranchNumberModel> tripBranchNumbers = tripBranchNumberList.FindAll(x => x.TripID  == trip.TripID);
                if (tripBranchNumbers.Count > 0)
                {
                    // グループごとにソート、追加
                    var tripBranchNumberSorted = AddTripBranchSeq(tripBranchNumbers);
                    results.AddRange(tripBranchNumberSorted);
                }
            }
            return results;
        }

        /// <summary>
        /// 便実績に該当するステーションに現在トラックが存在するか
        /// </summary>
        /// <param name="loadRecord">便実績</param>
        /// <returns></returns>
        public bool IsTruckExistedInSameStations(LoadRecordModel loadRecord)
        {
            // トラック有無取得SQL作成
            var isExistTrucksSQL = LoadProgressConnectController.CreateSQLToSelectIsExistTrucksFromStationID(loadRecord.StationID);
            // トラック有無取得
            IEnumerable<TruckExistModel> isExistTrucksList = ConnectToSQLServer.ExecuteQueryToList<TruckExistModel>(isExistTrucksSQL);
            if (isExistTrucksList.First() != null && isExistTrucksList.First().TruckExist)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 該当便実績は同ステーション内にて最新かどうか
        /// </summary>
        /// <param name="loadRecord">便実績</param>
        /// <returns></returns>
        public bool IsLatestRecordInSameStations(LoadRecordModel loadRecord)
        {
            var latestTripRecordsSQL = LoadProgressConnectController.CreateSQLToSelectLatestTripRecordsFromStationID(loadRecord.StationID);
            IEnumerable<LoadRecordModel> latestTripRecords = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(latestTripRecordsSQL);
            if (latestTripRecords.First() != null && latestTripRecords.First().TripRecordID == loadRecord.TripRecordID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
