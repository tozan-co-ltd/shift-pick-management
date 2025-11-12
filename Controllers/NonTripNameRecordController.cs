using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using JetBrains.Annotations;
using MathNet.Numerics;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Controllers
{
    public class NonTripNameRecordController : BaseController
    {
        public IActionResult Index()
        {
            var model = new NonTripNameRecordViewModel();
            // ログイン中ユーザー情報取得
            var user = ClaimsLoginUserData();
            model.UserName = user.UserName;
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;
            return View(model);
        }

        /// <summary>
        /// 便名称無し、識別番号有のデータのリストを取得する
        /// </summary>
        /// <param name="startOfPeriod">期間の開始日</param>
        /// <param name="endOfPeriod">期間の終了日</param>
        /// <param name="checkedDepos">選択されたデポリスト</param>
        /// <returns></returns>
        public List<NonTripNameRecordModel> GetNonTripNameRecordList(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var recordList = new List<NonTripNameRecordModel>();

            // 選択したデポの便リスト取得
            var tripSql = M_TripConnectController.CreateSQLToSelectMTrips(false, checkedDepos);
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(tripSql);

            // 便名称が無くて識別番号がある実績のリスト取得
            var nonTripNameSql = NonTripNameRecordConnectController.CreateSQLToSelectNonTripNameRecords(startOfPeriod, endOfPeriod, checkedDepos);
            var records = ConnectToSQLServer.ExecuteQueryToList<NonTripNameRecordModel>(nonTripNameSql);

            // 同実績の識別番号のみのリスト取得
            var identifyNumberSql = NonTripNameRecordConnectController.CreateSQLToSelectNonTripNameIdentifyNumbers(startOfPeriod, endOfPeriod, checkedDepos);
            var identifyNumbers = ConnectToSQLServer.ExecuteQueryToList<string>(identifyNumberSql);

            // 識別番号ごとに
            foreach (var identifyNumber in identifyNumbers)
            {
                // 識別番号に対応する実績リスト取得
                var sameIdentifyNumberRecords = records.FindAll(x => x.IdentifyNumber == identifyNumber);

                // 実績リストが空の場合
                if(sameIdentifyNumberRecords == null)
                    continue;

                // 識別番号に対応する便情報取得
                var trip = trips.Find(x =>  x.IdentifyNumber == identifyNumber);

                // 識別番号が同じ実績の便情報、便枝番情報を登録
                recordList = AddSameIdentifyNumberRecords(recordList, sameIdentifyNumberRecords, trip);

            }
            return recordList;
        }

        /// <summary>
        /// 識別番号が同じ実績の便情報、便枝番情報を登録
        /// </summary>
        /// <param name="recordList">全実績リスト</param>
        /// <param name="sameIdentifyNumberRecords">識別番号が同じ実績のリスト</param>
        /// <param name="trip">便情報</param>
        /// <returns></returns>
        private List<NonTripNameRecordModel> AddSameIdentifyNumberRecords(List<NonTripNameRecordModel> recordList, List<NonTripNameRecordModel> sameIdentifyNumberRecords, M_TripModel trip)
        {
            // 識別番号に対応する便情報が存在した場合
            if (trip != null)
            {
                // 便枝番リスト取得
                var tripBranchNumberSql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(trip.TripID, false);
                var tripBranchNumbers = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(tripBranchNumberSql);
                tripBranchNumbers = M_TripBranchNumberController.AddTripBranchSeq(tripBranchNumbers);

                foreach (var record in sameIdentifyNumberRecords)
                {
                    // 直近の便枝番情報取得
                    var addRecord = GetNearestTripBranchNumber(record, tripBranchNumbers, trip.TripName!);
                    recordList.Add(addRecord);
                }
            }
            // 識別番号に対応する便情報が存在しない場合
            else
            {
                foreach (var record in sameIdentifyNumberRecords)
                {
                    var addRecord = SettingRecordParameter(record, "なし", "-", "なし", "-");
                    recordList.Add(addRecord);
                }
            }
            return recordList;
        }

        /// <summary>
        /// 一番到着予定が近い便枝番の取得、パラメータ設定
        /// </summary>
        /// <param name="record">取得元の実績</param>
        /// <param name="tripBranchNumbers">取得元の便の便枝番リスト</param>
        /// <param name="tripName">取得元の便名称</param>
        /// <returns></returns>
        private NonTripNameRecordModel GetNearestTripBranchNumber(NonTripNameRecordModel record, List<M_TripBranchNumberModel> tripBranchNumbers, string tripName)
        {
            var comparisonTime = new DateTime(1900, 1, 1, record.ArrivedAt.Hour, record.ArrivedAt.Minute, record.ArrivedAt.Second);
            // 日付をまたがない場合に一番到着予定が近い便枝番取得
            var nearestBranchNumberToday = tripBranchNumbers.OrderBy(x => Math.Abs((x.ArrivalScheduledTime - comparisonTime).TotalSeconds)).First();
            // 日付をまたいだ場合に一番到着予定が近い便枝番取得
            var nearestBranchNumberNextDay = tripBranchNumbers.OrderBy(x => Math.Abs((x.ArrivalScheduledTime.AddDays(1) - comparisonTime).TotalSeconds)).First();

            // 一番到着予定が近い便枝番と到着予定時刻を取得
            var nearestBranchNumber = new M_TripBranchNumberModel();
            var arrivalScheduledTime = new DateTime();
            if (Math.Abs((nearestBranchNumberToday.ArrivalScheduledTime - comparisonTime).TotalSeconds) < Math.Abs((nearestBranchNumberNextDay.ArrivalScheduledTime.AddDays(1) - comparisonTime).TotalSeconds))
            {
                nearestBranchNumber = nearestBranchNumberToday;
                arrivalScheduledTime = nearestBranchNumber.ArrivalScheduledTime;
            }
            else
            {
                nearestBranchNumber = nearestBranchNumberNextDay;
                arrivalScheduledTime = nearestBranchNumber.ArrivalScheduledTime.AddDays(1);
            }

            // 各パラメータ設定
            record = SettingRecordParameter(record, tripName, nearestBranchNumber.ArrivalScheduledTime.ToString("HH:mm"), 
                nearestBranchNumber.TripBranchSeq.ToString(), ((int)(comparisonTime - arrivalScheduledTime).TotalMinutes).ToString());
 
            return record;
        }

        /// <summary>
        /// 便名称無し、識別番号有のデータのパラメータ登録
        /// </summary>
        /// <param name="record">設定したい実績</param>
        /// <param name="tripName">便名称</param>
        /// <param name="arrivalScheduledTime">到着予定時間</param>
        /// <param name="tripBranchNumber">便枝番</param>
        /// <param name="arrivalTimeDefference">到着予定との予実差</param>
        /// <returns></returns>
        private NonTripNameRecordModel SettingRecordParameter(NonTripNameRecordModel record, string tripName, string arrivalScheduledTime, string tripBranchNumber, string arrivalTimeDefference)
        {
            record.GuessTripName = tripName;
            record.NearestArrivaLScheduledTime =arrivalScheduledTime;
            record.GuessTripBranchNumber = tripBranchNumber;
            record.ArrivalTimeDefference = arrivalTimeDefference;
            return record;
        }

        /// <summary>
        /// テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var searchData = string.Empty;
            List<NonTripNameRecordModel> nonTripNameRecordList = new();
            try
            {
                if (checkedDepos.Count > 0)
                {
                    nonTripNameRecordList = GetNonTripNameRecordList(startOfPeriod, endOfPeriod, checkedDepos);
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">識別番号</th>
                                    <th class=""font-weight-bold"">到着実績</th>
                                    <th class=""font-weight-bold"">便名称</th>
                                    <th class=""font-weight-bold"">想定される便枝番</th>
                                    <th class=""font-weight-bold"">便枝番の到着予定</th>
                                    <th class=""font-weight-bold"">到着ズレ時間(分)</th>
                                    <th class=""font-weight-bold"">紐づけ切れ理由</th>
                                    <th class=""font-weight-bold"">到着荷量画像</th>
                                </tr>
                            </thead>
                            <tbody>
                ";
                // テーブルのbody部分
                if (nonTripNameRecordList.Count > 0)
                {
                    foreach (var nonTripNameRecord in nonTripNameRecordList)
                    {
                        searchData += $@"
                            <tr>
                                <td>{nonTripNameRecord.IdentifyNumber}</td>
                                <td>{nonTripNameRecord.ArrivedAt.ToString("yyyy/MM/dd HH:mm")}</td>
                                <td>{nonTripNameRecord.GuessTripName}</td>
                                <td>{nonTripNameRecord.GuessTripBranchNumber}</td>
                                <td>{nonTripNameRecord.NearestArrivaLScheduledTime}</td>
                                <td>{nonTripNameRecord.ArrivalTimeDefference}</td>
                                <td>a{nonTripNameRecord.Remarks}</td>
                                <td>
                                    <a class=""btn btn-success btn-icon-split ml-1 mr-1""
                                        onclick=""OnArrivalNonIdentifyNumberLoadImageClick('{nonTripNameRecord.TripRecordID}', this, 'NonIdentifyNumberRecord')"" data-id=""{nonTripNameRecord.TripRecordID}"" data-toggle=""modal"" data-target=""#detail-modal"">
                                        <i class=""fa-solid fa-truck""></i>
                                    </a>
                                </td>
                            </tr>
                        ";
                    }
                }
                searchData += $@"
                            </tbody>
                        </table>
                    </div>
                ";

                return Json(searchData);
            }
            catch (SqlException)
            {
                return Json(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Json(errorMessage);
            }
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            string? errorMessage;
            string startDate = startOfPeriod.ToString("yyyyMMdd");
            string endDate = endOfPeriod.ToString("yyyyMMdd");
            try
            {
                // 検索条件シート用データテーブル作成
                DataTable searchConditionDT = new DataTable();
                searchConditionDT.Columns.Add("項目名");
                searchConditionDT.Columns.Add("検索条件");
                // デポの設定
                var selectedDeposName = LoadRecordController.SelectedDepos(checkedDepos);
                searchConditionDT.Rows.Add("対象デポ", selectedDeposName);
                // 稼働日の設定
                searchConditionDT.Rows.Add("稼働日", $"{startDate}～{endDate}");

                var nonTripNameRecordList = new List<NonTripNameRecordModel>();

                if (checkedDepos.Count > 0)
                {
                    nonTripNameRecordList = GetNonTripNameRecordList(startOfPeriod, endOfPeriod, checkedDepos);
                }

                var nonTripNameRecordDT = ToDataTable<NonTripNameRecordModel>(nonTripNameRecordList);

                // 便実績が0の場合
                if (nonTripNameRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"月次紐づけ切れデータ_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "月次紐づけ切れデータ";
                string sheetNameTwo = "検索条件";


                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(nonTripNameRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, checkedDepos);

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
        /// List内のNullデータをDBNull.Valueとして登録する
        /// </summary>
        public DataTable ToDataTable<T>(IList<T> list)
        {
            var table = new DataTable();

            typeof(T).GetProperties().ToList().ForEach(
                p => table.Columns.Add(p.Name, typeof(string))
                );

            foreach (var item in list)
            {
                DataRow row = table.NewRow();
                typeof(T).GetProperties().ToList().ForEach(
                    p => row[p.Name] = p.GetValue(item) ?? DBNull.Value
                    );
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
