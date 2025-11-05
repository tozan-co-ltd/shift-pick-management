using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.Properties;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

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

        public List<NonTripNameRecordModel> GetNonTripNameRecordModel(DateTime startOfPeriod, DateTime endOfPeriod, List<string> checkedDepos)
        {
            var model = new List<NonTripNameRecordModel>();
            // 選択したデポの便リスト取得
            var tripSql = M_TripConnectController.CreateSQLToSelectMTrips(false, checkedDepos);
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(tripSql);

            // 便名称が無くて識別番号がある実績のリスト取得
            var nonTripNameSql = NonTripNameRecordConnectController.CreateSQLToSelectNonTripNameRecords(startOfPeriod, endOfPeriod, checkedDepos);
            var records = ConnectToSQLServer.ExecuteQueryToList<NonTripNameRecordModel>(nonTripNameSql);

            // 同実績の識別番号のみのリスト取得
            var identifyNumberSql = NonTripNameRecordConnectController.CreateSQLToSelectNonTripNameIdentifyNumbers(startOfPeriod, endOfPeriod, checkedDepos);
            var identifyNumbers = ConnectToSQLServer.ExecuteQueryToList<string>(identifyNumberSql);

            foreach (var identifyNumber in identifyNumbers)
            {
                var sameIdentifyNumberRecords = records.FindAll(x => x.IdentifyNumber == identifyNumber);
                if(sameIdentifyNumberRecords == null)
                    continue;
                var trip = trips.Find(x =>  x.IdentifyNumber == identifyNumber);
                if(trip != null)
                {
                    var tripBranchNumberSql = M_TripBranchNumberConnectController.CreateSQLToSelectMTripBranchNumbers(trip.TripID, false);
                    var tripBranchNumbers = ConnectToSQLServer.ExecuteQueryToList<M_TripBranchNumberModel>(tripBranchNumberSql);
                    tripBranchNumbers = M_TripBranchNumberController.AddTripBranchSeq(tripBranchNumbers);
                    foreach (var record in sameIdentifyNumberRecords)
                    {
                        var comparisonTime = new DateTime(1900, 1, 1, record.ArrivedAt.Hour, record.ArrivedAt.Minute, record.ArrivedAt.Second);
                        // 一番到着予定が近い便枝番取得
                        var nearestBranchNumber = tripBranchNumbers.OrderBy(x => Math.Abs((x.ArrivalScheduledTime - comparisonTime).TotalSeconds)).First();
                        // 到着予定と実績の差を取得
                        var defferentTime = (int)(comparisonTime - nearestBranchNumber.ArrivalScheduledTime).TotalMinutes;
                        // 各パラメータ設定
                        record.GuessTripName = trip.TripName;
                        record.NearestArrivaLScheduledTime = nearestBranchNumber.ArrivalScheduledTime.ToString("HH:mm");
                        record.GuessTripBranchNumber = nearestBranchNumber.TripBranchSeq.ToString();
                        record.ArrivalTimeDefference = defferentTime.ToString();
                        model.Add(record);
                    }
                }
                else
                {
                    foreach (var record in sameIdentifyNumberRecords)
                    {
                        record.GuessTripName = "なし";
                        record.NearestArrivaLScheduledTime = "なし";
                        record.GuessTripBranchNumber = "なし";
                        record.ArrivalTimeDefference = "なし";
                        model.Add(record);
                    }
                }

            }
            return model;
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
                    nonTripNameRecordList = GetNonTripNameRecordModel(startOfPeriod, endOfPeriod, checkedDepos);
                }

                // テーブルのヘッダ部分
                searchData += $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center"" id=""tripRecordDataTable"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">識別番号</th>
                                    <th class=""font-weight-bold"">到着実績</th>
                                    <th class=""font-weight-bold"">想定される便</th>
                                    <th class=""font-weight-bold"">直近の到着予定</th>
                                    <th class=""font-weight-bold"">到着ズレ時間(分)</th>
                                    <th class=""font-weight-bold"">想定される便枝番</th>
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
                                <td>{nonTripNameRecord.NearestArrivaLScheduledTime}</td>
                                <td>{nonTripNameRecord.ArrivalTimeDefference}</td>
                                <td>{nonTripNameRecord.GuessTripBranchNumber}</td>
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
                    nonTripNameRecordList = GetNonTripNameRecordModel(startOfPeriod, endOfPeriod, checkedDepos);
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
                p => table.Columns.Add(p.Name,
                                       Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)
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
